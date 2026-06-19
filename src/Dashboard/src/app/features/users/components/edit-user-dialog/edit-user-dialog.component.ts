import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardUser } from '../../models/dashboard-user.model';

@Component({
  selector: 'app-edit-user-dialog',
  imports: [
    ReactiveFormsModule,
    DialogActionBarComponent,
    DialogShellComponent,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule
  ],
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
  readonly userForm = this.createUserForm(this.dialogData);

  closeDialog(): void {
    this.dialogRef.close();
  }

  saveUser(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    console.log('User edit payload', this.buildUserEditPayload());
  }

  saveUserAndClose(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    console.log('User edit payload', this.buildUserEditPayload());
    this.dialogRef.close(true);
  }

  private createUserForm(initialUser: DashboardUser | null) {
    return this.formBuilder.nonNullable.group({
      id: this.formBuilder.nonNullable.control(initialUser?.id ?? ''),
      numberId: this.formBuilder.nonNullable.control(initialUser?.numberId ?? 0),
      username: this.formBuilder.nonNullable.control(initialUser?.username ?? ''),
      email: this.formBuilder.nonNullable.control(initialUser?.email ?? ''),
      phoneNumber: this.formBuilder.nonNullable.control(initialUser?.phoneNumber ?? ''),
      isEmailConfirmed: this.formBuilder.nonNullable.control(initialUser?.isEmailConfirmed ?? false),
      isPhoneNumberConfirmed: this.formBuilder.nonNullable.control(
        initialUser?.isPhoneNumberConfirmed ?? false
      ),
      lastLoginAt: this.formBuilder.nonNullable.control(initialUser?.lastLoginAt ?? '')
    });
  }

  private buildUserEditPayload(): Readonly<Record<string, unknown>> {
    return {
      id: this.dialogData?.id ?? null,
      numberId: this.dialogData?.numberId ?? null,
      user: this.dialogData,
      form: this.userForm.getRawValue()
    };
  }

  formatLastLoginAt(value: string): string {
    if (value.length === 0) {
      return 'Never';
    }

    const parsedDate = new Date(value);

    return Number.isNaN(parsedDate.getTime()) ? value : parsedDate.toLocaleString();
  }
}
