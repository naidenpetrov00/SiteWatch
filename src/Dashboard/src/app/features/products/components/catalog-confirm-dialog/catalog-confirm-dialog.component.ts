import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

export interface CatalogConfirmDialogData {
  title: string;
  message: string;
  confirmLabel: string;
}

@Component({
  selector: 'app-product-catalog-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './catalog-confirm-dialog.component.html',
  styleUrl: './catalog-confirm-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogConfirmDialogComponent {
  readonly data = inject<CatalogConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CatalogConfirmDialogComponent, boolean>);

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
