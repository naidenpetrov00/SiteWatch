import { FormBuilder, FormControl, FormGroup } from '@angular/forms';

import { DashboardUser } from '../../models/dashboard-user.model';

export const EDIT_USER_DIALOG_FORM_ID = 'edit-user-dialog-form';

export type EditUserDialogForm = FormGroup<{
  id: FormControl<string>;
  numberId: FormControl<number>;
  username: FormControl<string>;
  email: FormControl<string>;
  phoneNumber: FormControl<string>;
  isEmailConfirmed: FormControl<boolean>;
  isPhoneNumberConfirmed: FormControl<boolean>;
  lastLoginAt: FormControl<string>;
}>;

export function createEditUserDialogForm(
  formBuilder: FormBuilder,
  initialUser: DashboardUser | null
): EditUserDialogForm {
  return formBuilder.nonNullable.group({
    id: formBuilder.nonNullable.control(initialUser?.id ?? ''),
    numberId: formBuilder.nonNullable.control(initialUser?.numberId ?? 0),
    username: formBuilder.nonNullable.control(initialUser?.username ?? ''),
    email: formBuilder.nonNullable.control(initialUser?.email ?? ''),
    phoneNumber: formBuilder.nonNullable.control(initialUser?.phoneNumber ?? ''),
    isEmailConfirmed: formBuilder.nonNullable.control(initialUser?.isEmailConfirmed ?? false),
    isPhoneNumberConfirmed: formBuilder.nonNullable.control(
      initialUser?.isPhoneNumberConfirmed ?? false
    ),
    lastLoginAt: formBuilder.nonNullable.control(initialUser?.lastLoginAt ?? '')
  });
}

export function buildUserEditPayload(
  dialogData: DashboardUser | null,
  userForm: EditUserDialogForm
): Readonly<Record<string, unknown>> {
  return {
    id: dialogData?.id ?? null,
    numberId: dialogData?.numberId ?? null,
    user: dialogData,
    form: userForm.getRawValue()
  };
}

export function formatLastLoginAt(value: string): string {
  if (value.length === 0) {
    return 'Never';
  }

  const parsedDate = new Date(value);

  return Number.isNaN(parsedDate.getTime()) ? value : parsedDate.toLocaleString();
}
