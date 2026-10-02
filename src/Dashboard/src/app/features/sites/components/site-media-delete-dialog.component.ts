import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../shared/ui/dialog-shell/dialog-shell.component';

@Component({
  selector: 'app-site-media-delete-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  template: `<app-dialog-shell eyebrow="Site media" title="Delete item" subtitle="This action cannot be undone."><p>Delete {{ label }} permanently?</p><app-dialog-action-bar dialog-shell-actions cancelLabel="Cancel" submitLabel="Delete" (cancel)="close(false)" (submit)="close(true)" /></app-dialog-shell>`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SiteMediaDeleteDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<SiteMediaDeleteDialogComponent>);
  readonly label = inject<string>(MAT_DIALOG_DATA);
  close(result: boolean): void { this.dialogRef.close(result); }
}
