import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface OfferArchiveConfirmDialogData {
  offerNumber: number;
}

@Component({
  selector: 'app-offer-archive-confirm-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './offer-archive-confirm-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferArchiveConfirmDialogComponent {
  private readonly dialogRef = inject(
    MatDialogRef<OfferArchiveConfirmDialogComponent>
  );
  readonly data = inject<OfferArchiveConfirmDialogData>(MAT_DIALOG_DATA);

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
