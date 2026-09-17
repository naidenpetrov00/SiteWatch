import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface RetailerStatusConfirmDialogData {
  displayName: string;
  activate: boolean;
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

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
