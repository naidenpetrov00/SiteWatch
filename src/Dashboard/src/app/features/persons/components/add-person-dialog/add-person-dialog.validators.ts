import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const ADD_PERSON_VALIDATION_LIMITS = {
  firstName: 100,
  middleName: 100,
  lastName: 100,
  companyName: 250,
  egnLength: 10,
  eikMinLength: 9,
  eikMaxLength: 13,
  vatNumberLength: 20,
  addressLine: 200,
  additionalLine: 200,
  city: 100,
  postalCode: 20,
  country: 100,
  details: 500,
  contactValue: 256,
  contactDetails: 500,
  iban: 34,
  bic: 11,
  bankName: 200,
  bankDetails: 500
} as const;

function hasText(value: unknown): boolean {
  return typeof value === 'string' && value.trim().length > 0;
}

function hasRepeatableContent(value: unknown, defaultValue: unknown): boolean {
  if (typeof defaultValue === 'string' && typeof value === 'string') {
    return value !== defaultValue;
  }

  return value !== defaultValue;
}

export function requiredTextValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    return hasText(control.value) ? null : { required: true };
  };
}

export function digitsOnlyValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string' || value.length === 0) {
      return null;
    }

    return /^\d+$/.test(value) ? null : { digitsOnly: true };
  };
}

interface RepeatableRowValidatorOptions {
  errorKey: string;
  requiredField: string;
  presenceDefaults: Readonly<Record<string, unknown>>;
}

export function createRepeatableRowValidator(
  options: RepeatableRowValidatorOptions
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as Record<string, unknown> | null;

    if (!value) {
      return null;
    }

    const hasContent = Object.entries(options.presenceDefaults).some(([field, defaultValue]) =>
      hasRepeatableContent(value[field], defaultValue)
    );

    if (!hasContent) {
      return null;
    }

    return hasText(value[options.requiredField]) ? null : { [options.errorKey]: true };
  };
}

export function createAtMostOnePrimaryValidator(
  errorKey: string,
  primaryField = 'isPrimary'
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as ReadonlyArray<Record<string, unknown>> | null;

    if (!Array.isArray(value)) {
      return null;
    }

    const primaryCount = value.filter((item) => item?.[primaryField] === true).length;

    return primaryCount <= 1 ? null : { [errorKey]: true };
  };
}
