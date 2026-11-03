import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface RetailerStatusConfirmDialogData {
  displayName: string;
  activate: boolean;
  removesFromOfferDefaults: boolean;
}

@Component({
  selector: 'app-retailer-status-confirm-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './retailer-status-confirm-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerStatusConfirmDialogComponent {
  private readonly dialogRef = inject(
    MatDialogRef<RetailerStatusConfirmDialogComponent>
  );
  readonly data = inject<RetailerStatusConfirmDialogData>(MAT_DIALOG_DATA);
  readonly subtitle = this.data.activate
    ? 'The retailer will become available to future listing and offer workflows.'
    : this.data.removesFromOfferDefaults
      ? 'The retailer will be removed from defaults for future Offers. Existing Offers remain unchanged.'
      : 'The retailer remains available for historical references and administration.';

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
