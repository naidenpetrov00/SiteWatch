import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const UUID_REGEX =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const DECIMAL_REGEX = /^\d+(\.\d{1,2})?$/;
const DATE_REGEX = /^\d{4}-\d{2}-\d{2}$/;
const DATE_TIME_REGEX = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2})?$/;

export function uuidValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string' || value.trim().length === 0) {
      return null;
    }

    return UUID_REGEX.test(value.trim()) ? null : { uuid: true };
  };
}

export function decimalValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    if (value.trim().length === 0) {
      return null;
    }

    return DECIMAL_REGEX.test(value.trim()) ? null : { decimal: true };
  };
}

export function positiveDecimalValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    const normalizedValue = value.trim();

    if (normalizedValue.length === 0) {
      return null;
    }

    const parsedValue = Number(normalizedValue);

    if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
      return { positiveDecimal: true };
    }

    return DECIMAL_REGEX.test(normalizedValue) ? null : { positiveDecimal: true };
  };
}

export function dateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    if (value.trim().length === 0) {
      return null;
    }

    return DATE_REGEX.test(value.trim()) ? null : { date: true };
  };
}

export function dateTimeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (typeof value !== 'string') {
      return null;
    }

    if (value.trim().length === 0) {
      return null;
    }

    return DATE_TIME_REGEX.test(value.trim()) ? null : { dateTime: true };
  };
}
