import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';

export interface ExtractionProfileConfirmDialogData {
  eyebrow: string;
  title: string;
  subtitle: string;
  message: string;
  confirmLabel: string;
}

@Component({
  selector: 'app-extraction-profile-confirm-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './extraction-profile-confirm-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExtractionProfileConfirmDialogComponent {
  private readonly dialogRef = inject(
    MatDialogRef<ExtractionProfileConfirmDialogComponent>
  );
  readonly data = inject<ExtractionProfileConfirmDialogData>(MAT_DIALOG_DATA);

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
