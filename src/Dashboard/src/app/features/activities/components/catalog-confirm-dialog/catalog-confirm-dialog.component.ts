import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface CatalogConfirmDialogData {
  title: string;
  subtitle: string;
  message: string;
  confirmLabel: string;
}

@Component({
  selector: 'app-catalog-confirm-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './catalog-confirm-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogConfirmDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CatalogConfirmDialogComponent>);
  readonly data = inject<CatalogConfirmDialogData>(MAT_DIALOG_DATA);

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
