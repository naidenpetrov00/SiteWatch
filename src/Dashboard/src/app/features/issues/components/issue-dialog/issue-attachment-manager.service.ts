import { Injectable, inject, signal } from '@angular/core';

import { IssueAttachmentAccessService } from '../../services/issue-attachment-access.service';
import { DashboardIssuesService } from '../../services/dashboard-issues.service';
import { getIssueAttachmentErrorMessage } from './issue-attachment-error.utils';
import { validateIssueAttachmentFile } from './issue-attachment-file-validation';
import {
  IssueAttachmentViewer,
  ManagedIssueAttachment,
  QueuedIssueAttachment
} from './issue-attachment-manager.types';
import { IssueAttachmentObjectUrlRegistry } from './issue-attachment-object-url-registry.service';
import { IssueAttachmentUploader } from './issue-attachment-uploader.service';

@Injectable()
export class IssueAttachmentManager {
  private readonly dashboardIssuesService = inject(DashboardIssuesService);
  private readonly attachmentAccess = inject(IssueAttachmentAccessService);
  private readonly uploader = inject(IssueAttachmentUploader);
  private readonly objectUrls = inject(IssueAttachmentObjectUrlRegistry);
  private nextClientId = 0;

  private readonly _existingAttachments = signal<readonly ManagedIssueAttachment[]>([]);
  private readonly _queuedAttachments = signal<readonly QueuedIssueAttachment[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _isPersisting = signal(false);
  private readonly _loadError = signal<string | null>(null);
  private readonly _selectionError = signal<string | null>(null);
  private readonly _saveError = signal<string | null>(null);
  private readonly _viewer = signal<IssueAttachmentViewer | null>(null);

  readonly existingAttachments = this._existingAttachments.asReadonly();
  readonly queuedAttachments = this._queuedAttachments.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly isPersisting = this._isPersisting.asReadonly();
  readonly loadError = this._loadError.asReadonly();
  readonly selectionError = this._selectionError.asReadonly();
  readonly saveError = this._saveError.asReadonly();
  readonly viewer = this._viewer.asReadonly();

  async load(issueId: string): Promise<void> {
    if (this._isLoading()) {
      return;
    }

    this._isLoading.set(true);
    this._loadError.set(null);
    try {
      const attachments = await this.dashboardIssuesService.getIssueAttachments(issueId);
      this._existingAttachments.set(attachments.map((attachment) => ({
        attachment,
        markedForRemoval: false,
        status: 'idle',
        error: null,
        previewUrl: null
      })));

      for (const attachment of attachments) {
        if (attachment.hasPreview) {
          void this.loadPreview(issueId, attachment.id);
        }
      }
    } catch (error) {
      this._loadError.set(getIssueAttachmentErrorMessage(
        error,
        'Unable to load issue attachments.'
      ));
    } finally {
      this._isLoading.set(false);
    }
  }

  queueFiles(files: FileList | readonly File[]): void {
    this._selectionError.set(null);
    const accepted: QueuedIssueAttachment[] = [];
    const errors: string[] = [];

    for (const file of Array.from(files)) {
      const validation = validateIssueAttachmentFile(file);
      if (typeof validation === 'string') {
        errors.push(`${file.name}: ${validation}`);
        continue;
      }

      accepted.push({
        clientId: `issue-attachment-${Date.now()}-${this.nextClientId++}`,
        file,
        kind: validation.kind,
        contentType: validation.contentType,
        previewUrl: validation.kind === 'File' ? null : this.objectUrls.create(file),
        status: 'queued',
        progress: null,
        error: null
      });
    }

    if (accepted.length > 0) {
      this._queuedAttachments.update((current) => [...current, ...accepted]);
    }
    if (errors.length > 0) {
      this._selectionError.set(errors.join(' '));
    }
  }

  removeQueuedAttachment(clientId: string): void {
    const item = this._queuedAttachments().find((candidate) => candidate.clientId === clientId);
    if (!item || item.status === 'uploading') {
      return;
    }

    this.objectUrls.release(item.previewUrl);
    this._queuedAttachments.update((current) =>
      current.filter((candidate) => candidate.clientId !== clientId)
    );
  }

  toggleExistingRemoval(attachmentId: string): void {
    if (this._isPersisting()) {
      return;
    }

    this.updateExisting(attachmentId, (item) => ({
      ...item,
      markedForRemoval: !item.markedForRemoval,
      status: 'idle',
      error: null
    }));
  }

  async saveChanges(issueId: string): Promise<boolean> {
    this._isPersisting.set(true);
    this._saveError.set(null);
    let succeeded = true;

    try {
      const removals = this._existingAttachments()
        .filter((item) => item.markedForRemoval)
        .map((item) => item.attachment.id);
      for (const attachmentId of removals) {
        if (!await this.deleteExisting(issueId, attachmentId)) {
          succeeded = false;
        }
      }

      const uploads = this._queuedAttachments()
        .filter((item) => item.status === 'queued' || item.status === 'error')
        .map((item) => item.clientId);
      for (const clientId of uploads) {
        if (!await this.uploadQueued(issueId, clientId)) {
          succeeded = false;
        }
      }
    } finally {
      this._isPersisting.set(false);
    }

    if (!succeeded) {
      this._saveError.set(
        'Some attachment changes could not be completed. Review the errors and save again to retry.'
      );
    }

    return succeeded;
  }

  async openAttachment(issueId: string, item: ManagedIssueAttachment): Promise<void> {
    if (item.attachment.kind === 'File') {
      await this.downloadAttachment(issueId, item);
      return;
    }

    try {
      const url = await this.attachmentAccess.getViewUrl(
        issueId,
        item.attachment.id
      );
      this._viewer.set({
        kind: item.attachment.kind,
        name: item.attachment.fileName,
        url
      });
    } catch (error) {
      this.updateExisting(item.attachment.id, (current) => ({
        ...current,
        error: getIssueAttachmentErrorMessage(error, 'Unable to open this attachment.')
      }));
    }
  }

  async downloadAttachment(issueId: string, item: ManagedIssueAttachment): Promise<void> {
    try {
      await this.attachmentAccess.download(issueId, item.attachment);
    } catch (error) {
      this.updateExisting(item.attachment.id, (current) => ({
        ...current,
        error: getIssueAttachmentErrorMessage(error, 'Unable to download this attachment.')
      }));
    }
  }

  closeViewer(): void {
    this._viewer.set(null);
  }

  openQueuedAttachment(item: QueuedIssueAttachment): void {
    if (!item.previewUrl || item.kind === 'File') {
      return;
    }

    this._viewer.set({
      kind: item.kind,
      name: item.file.name,
      url: item.previewUrl
    });
  }

  private async deleteExisting(issueId: string, attachmentId: string): Promise<boolean> {
    this.updateExisting(attachmentId, (item) => ({
      ...item,
      status: 'deleting',
      error: null
    }));
    try {
      await this.dashboardIssuesService.deleteIssueAttachment(issueId, attachmentId);
      const removed = this._existingAttachments().find(
        (item) => item.attachment.id === attachmentId
      );
      this.objectUrls.release(removed?.previewUrl ?? null);
      this._existingAttachments.update((current) =>
        current.filter((item) => item.attachment.id !== attachmentId)
      );
      return true;
    } catch (error) {
      this.updateExisting(attachmentId, (item) => ({
        ...item,
        status: 'error',
        error: getIssueAttachmentErrorMessage(error, 'Unable to delete this attachment.')
      }));
      return false;
    }
  }

  private async uploadQueued(issueId: string, clientId: string): Promise<boolean> {
    const queued = this._queuedAttachments().find((item) => item.clientId === clientId);
    if (!queued) {
      return true;
    }

    this.updateQueued(clientId, (item) => ({
      ...item,
      status: 'uploading',
      progress: 0,
      error: null
    }));

    try {
      const attachment = await this.uploader.upload(
        issueId,
        queued.file,
        (progress) => this.updateQueued(
          clientId,
          (item) => ({ ...item, progress })
        )
      );
      const latest = this._queuedAttachments().find(
        (item) => item.clientId === clientId
      );
      this._queuedAttachments.update((current) =>
        current.filter((item) => item.clientId !== clientId)
      );
      this._existingAttachments.update((current) => [{
        attachment,
        markedForRemoval: false,
        status: 'idle',
        error: null,
        previewUrl: latest?.previewUrl ?? null
      }, ...current]);
      if (attachment.hasPreview) {
        void this.loadPreview(issueId, attachment.id);
      }
      return true;
    } catch (error) {
      this.updateQueued(clientId, (item) => ({
        ...item,
        status: 'error',
        progress: null,
        error: getIssueAttachmentErrorMessage(
          error,
          'Unable to upload this attachment.'
        )
      }));
      return false;
    }
  }

  private async loadPreview(issueId: string, attachmentId: string): Promise<void> {
    try {
      const previewUrl = await this.attachmentAccess.getPreviewUrl(
        issueId,
        attachmentId
      );
      const current = this._existingAttachments().find(
        (item) => item.attachment.id === attachmentId
      );
      this.objectUrls.release(current?.previewUrl ?? null);
      this.updateExisting(attachmentId, (item) => ({
        ...item,
        previewUrl
      }));
    } catch {
      // A preview failure leaves the file card available for retryable open/download actions.
    }
  }

  private updateExisting(
    attachmentId: string,
    update: (item: ManagedIssueAttachment) => ManagedIssueAttachment
  ): void {
    this._existingAttachments.update((current) => current.map((item) =>
      item.attachment.id === attachmentId ? update(item) : item
    ));
  }

  private updateQueued(
    clientId: string,
    update: (item: QueuedIssueAttachment) => QueuedIssueAttachment
  ): void {
    this._queuedAttachments.update((current) => current.map((item) =>
      item.clientId === clientId ? update(item) : item
    ));
  }
}
