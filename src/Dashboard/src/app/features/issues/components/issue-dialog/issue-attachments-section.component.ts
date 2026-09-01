import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import {
  IssueAttachmentManager
} from './issue-attachment-manager.service';
import {
  ManagedIssueAttachment,
  QueuedIssueAttachment
} from './issue-attachment-manager.types';
import { getIssueAttachmentFileIcon } from './issue-attachment-file-presentation';

@Component({
  selector: 'app-issue-attachments-section',
  imports: [MatButtonModule, MatIconModule, MatProgressBarModule],
  templateUrl: './issue-attachments-section.component.html',
  styleUrl: './issue-attachments-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IssueAttachmentsSectionComponent {
  readonly attachmentManager = inject(IssueAttachmentManager);
  readonly fileIcon = getIssueAttachmentFileIcon;
  readonly issueId = input<string | null>(null);
  readonly disabled = input(false);
  readonly isDragging = signal(false);
  readonly interactionsDisabled = computed(
    () => this.disabled() || this.attachmentManager.isPersisting()
  );

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.attachmentManager.queueFiles(input.files);
    }
    input.value = '';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.interactionsDisabled()) {
      this.isDragging.set(true);
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    if (!this.interactionsDisabled() && event.dataTransfer?.files) {
      this.attachmentManager.queueFiles(event.dataTransfer.files);
    }
  }

  formatSize(sizeBytes: number): string {
    if (sizeBytes < 1024) return `${sizeBytes} B`;
    if (sizeBytes < 1024 * 1024) return `${(sizeBytes / 1024).toFixed(1)} KB`;
    return `${(sizeBytes / 1024 / 1024).toFixed(1)} MB`;
  }

  formatDuration(durationSeconds: number | null): string | null {
    if (durationSeconds === null) return null;
    const minutes = Math.floor(durationSeconds / 60);
    const seconds = durationSeconds % 60;
    return `${minutes}:${String(seconds).padStart(2, '0')}`;
  }

  openAttachment(item: ManagedIssueAttachment): void {
    const issueId = this.currentIssueId();
    if (issueId) void this.attachmentManager.openAttachment(issueId, item);
  }

  openQueuedAttachment(item: QueuedIssueAttachment): void {
    this.attachmentManager.openQueuedAttachment(item);
  }

  downloadAttachment(item: ManagedIssueAttachment): void {
    const issueId = this.currentIssueId();
    if (issueId) void this.attachmentManager.downloadAttachment(issueId, item);
  }

  retryLoad(): void {
    const issueId = this.currentIssueId();
    if (issueId) void this.attachmentManager.load(issueId);
  }

  private currentIssueId(): string | null {
    return this.issueId();
  }
}
