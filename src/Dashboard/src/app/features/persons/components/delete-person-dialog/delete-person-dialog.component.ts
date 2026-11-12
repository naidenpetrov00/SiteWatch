import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface DeletePersonDialogData {
  displayName: string;
  numberId: number;
}

@Component({
  selector: 'app-delete-person-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './delete-person-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeletePersonDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<DeletePersonDialogComponent>);
  readonly person = inject(MAT_DIALOG_DATA) as DeletePersonDialogData;

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
