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
  phoneValueMinLength: 8,
  phoneValueMaxLength: 15,
  contactDetails: 500,
  iban: 34,
  bic: 11,
  bankName: 200,
  bankDetails: 500
} as const;

const LETTERS_ONLY_REGEX = /^[\p{L}\p{M}]+$/u;
const COMPANY_NAME_REGEX = /^[\p{L}\p{M}\d ]+$/u;
const DIGITS_ONLY_REGEX = /^\d+$/;
const WEBSITE_PROTOCOL_REGEX = /^[a-z][a-z0-9+.-]*:\/\//i;

function hasText(value: unknown): boolean {
  return typeof value === 'string' && value.trim().length > 0;
}

function hasRepeatableContent(value: unknown, defaultValue: unknown): boolean {
  if (typeof defaultValue === 'string' && typeof value === 'string') {
    return value !== defaultValue;
  }

  return value !== defaultValue;
}

export function lettersOnlyValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    if (value.length === 0) {
      return null;
    }

    return LETTERS_ONLY_REGEX.test(value) ? null : { lettersOnly: true };
  };
}

export function companyNameValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    if (value.trim().length === 0) {
      return { companyName: true };
    }

    return COMPANY_NAME_REGEX.test(value) ? null : { companyName: true };
  };
}

export function digitsOnlyValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    if (value.length === 0) {
      return null;
    }

    return DIGITS_ONLY_REGEX.test(value) ? null : { digitsOnly: true };
  };
}

export function websiteValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    const trimmedValue = value.trim();

    if (trimmedValue.length === 0) {
      return null;
    }

    return isValidWebsiteValue(trimmedValue) ? null : { website: true };
  };
}

function isValidWebsiteValue(value: string): boolean {
  const candidate = WEBSITE_PROTOCOL_REGEX.test(value) ? value : `https://${value}`;

  try {
    const url = new URL(candidate);
    return url.hostname === 'localhost' || url.hostname.includes('.');
  } catch {
    return false;
  }
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
