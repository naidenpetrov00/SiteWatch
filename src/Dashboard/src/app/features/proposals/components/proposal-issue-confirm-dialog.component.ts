import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../shared/ui/dialog-shell/dialog-shell.component';

export interface ProposalIssueConfirmDialogData {
  proposalNumber: number;
  revisionNumber: number;
}

@Component({
  selector: 'app-proposal-issue-confirm-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  template: `
    <app-dialog-shell
      eyebrow="Proposal Lifecycle"
      title="Issue Proposal"
      subtitle="Issuance stores the permanent PDF and makes this revision immutable."
    >
      <p>
        Proposal #{{ data.proposalNumber }}, revision {{ data.revisionNumber }},
        is ready to be issued.
      </p>
      <app-dialog-action-bar
        dialog-shell-actions
        cancelLabel="Cancel"
        submitLabel="Issue Proposal"
        (cancel)="cancel()"
        (submit)="confirm()"
      />
    </app-dialog-shell>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalIssueConfirmDialogComponent {
  private readonly dialogRef = inject(
    MatDialogRef<ProposalIssueConfirmDialogComponent>
  );
  readonly data = inject<ProposalIssueConfirmDialogData>(MAT_DIALOG_DATA);

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
