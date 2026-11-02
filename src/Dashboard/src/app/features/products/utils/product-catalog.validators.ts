import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function trimmedRequiredValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    typeof control.value === 'string' && control.value.trim().length > 0
      ? null
      : { required: true };
}
