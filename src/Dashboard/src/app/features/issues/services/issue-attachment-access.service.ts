import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';

import { IssueAttachment } from '../models/issue-attachment.model';
import { DashboardIssuesService } from './dashboard-issues.service';

@Injectable({ providedIn: 'root' })
export class IssueAttachmentAccessService {
  private readonly dashboardIssuesService = inject(DashboardIssuesService);
  private readonly document = inject(DOCUMENT);

  async getPreviewUrl(issueId: string, attachmentId: string): Promise<string> {
    const access = await this.dashboardIssuesService.getIssueAttachmentAccess(
      issueId,
      attachmentId,
      true,
      false
    );
    return access.url;
  }

  async getViewUrl(issueId: string, attachmentId: string): Promise<string> {
    const access = await this.dashboardIssuesService.getIssueAttachmentAccess(
      issueId,
      attachmentId,
      false,
      false
    );
    return access.url;
  }

  async download(issueId: string, attachment: IssueAttachment): Promise<void> {
    const access = await this.dashboardIssuesService.getIssueAttachmentAccess(
      issueId,
      attachment.id,
      false,
      true
    );
    const anchor = this.document.createElement('a');
    anchor.href = access.url;
    anchor.download = attachment.fileName;
    anchor.rel = 'noopener';
    this.document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
  }
}
