import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface DeleteCameraDialogData { name: string; numberId: number; }

@Component({
  selector: 'app-delete-camera-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './delete-camera-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeleteCameraDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<DeleteCameraDialogComponent>);
  readonly camera = inject(MAT_DIALOG_DATA) as DeleteCameraDialogData;
  cancel(): void { this.dialogRef.close(false); }
  confirm(): void { this.dialogRef.close(true); }
}
