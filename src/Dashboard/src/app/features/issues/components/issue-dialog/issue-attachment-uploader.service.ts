import { HttpEventType, HttpResponse } from '@angular/common/http';
import { DestroyRef, Injectable, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { IssueAttachment } from '../../models/issue-attachment.model';
import { DashboardIssuesService } from '../../services/dashboard-issues.service';

@Injectable()
export class IssueAttachmentUploader {
  private readonly dashboardIssuesService = inject(DashboardIssuesService);
  private readonly destroyRef = inject(DestroyRef);

  upload(
    issueId: string,
    file: File,
    onProgress: (progress: number | null) => void
  ): Promise<IssueAttachment> {
    return new Promise<IssueAttachment>((resolve, reject) => {
      let settled = false;

      this.dashboardIssuesService.uploadIssueAttachment(issueId, file)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (event) => {
            if (event.type === HttpEventType.UploadProgress) {
              onProgress(event.total
                ? Math.round((event.loaded / event.total) * 100)
                : null);
              return;
            }

            if (event instanceof HttpResponse && event.body) {
              settled = true;
              resolve(event.body);
            }
          },
          error: (error) => {
            settled = true;
            reject(error);
          },
          complete: () => {
            if (!settled) {
              reject(new Error('Upload completed without attachment metadata.'));
            }
          }
        });
    });
  }
}
