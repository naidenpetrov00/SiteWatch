import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import {
  EDIT_USER_DIALOG_FORM_ID,
  EditUserDialogForm,
  formatLastLoginAt
} from './edit-user-dialog.helpers';

@Component({
  selector: 'app-edit-user-dialog-content',
  imports: [ReactiveFormsModule, MatCheckboxModule, MatFormFieldModule, MatInputModule],
  templateUrl: './edit-user-dialog-content.component.html',
  styleUrl: './edit-user-dialog-content.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditUserDialogContentComponent {
  readonly userForm = input.required<EditUserDialogForm>();
  readonly saveRequested = output<void>();
  readonly formId = EDIT_USER_DIALOG_FORM_ID;
  readonly formatLastLoginAt = formatLastLoginAt;

  onSubmit(): void {
    this.saveRequested.emit();
  }
}
