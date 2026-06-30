import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardUser } from '../../models/dashboard-user.model';
import { EditUserDialogContentComponent } from './edit-user-dialog-content.component';
import {
  EDIT_USER_DIALOG_FORM_ID,
  buildUserEditPayload,
  createEditUserDialogForm
} from './edit-user-dialog.helpers';

@Component({
  selector: 'app-edit-user-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent, EditUserDialogContentComponent],
  templateUrl: './edit-user-dialog.component.html',
  styleUrl: './edit-user-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditUserDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<EditUserDialogComponent>);
  private readonly dialogData = inject(MAT_DIALOG_DATA, { optional: true }) as DashboardUser | null;

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = this.dialogData ? `Edit User #${this.dialogData.numberId}` : 'Edit User';
  readonly dialogSubtitle =
    'Review the current user details, adjust the fields you need, and keep the changes local for now.';
  readonly submitLabel = 'Save and close';
  readonly secondaryLabel = 'Save';
  readonly formId = EDIT_USER_DIALOG_FORM_ID;
  readonly userForm = createEditUserDialogForm(this.formBuilder, this.dialogData);

  closeDialog(): void {
    this.dialogRef.close();
  }

  saveUser(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    console.log('User edit payload', buildUserEditPayload(this.dialogData, this.userForm));
  }

  saveUserAndClose(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    console.log('User edit payload', buildUserEditPayload(this.dialogData, this.userForm));
    this.dialogRef.close(true);
  }
}
