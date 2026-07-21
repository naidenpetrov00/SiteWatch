import { PHONE_COUNTRY_CODE_OPTIONS, AddPersonPhoneCountryCodeOption } from './add-person-dialog.types';

export function sanitizeLettersOnly(value: string): string {
  return value.normalize('NFC').replace(/[^\p{L}\p{M}]+/gu, '');
}

export function sanitizeCompanyName(value: string): string {
  return value.normalize('NFC').replace(/[^\p{L}\p{M}\d ]+/gu, '');
}

export function sanitizeDigitsOnly(value: string): string {
  return value.replace(/\D+/g, '');
}

export function sanitizePhoneNationalNumber(value: string): string {
  return sanitizeDigitsOnly(value).replace(/^0+/, '');
}

export function combinePhoneValue(
  countryCode: AddPersonPhoneCountryCodeOption,
  nationalNumber: string
): string {
  const sanitizedNationalNumber = sanitizePhoneNationalNumber(nationalNumber);

  return sanitizedNationalNumber.length > 0 ? `${countryCode}${sanitizedNationalNumber}` : '';
}

export function splitPhoneValue(value: string): {
  countryCode: AddPersonPhoneCountryCodeOption;
  nationalNumber: string;
} {
  const sanitizedValue = sanitizeDigitsOnly(value);

  if (sanitizedValue.length === 0) {
    return {
      countryCode: PHONE_COUNTRY_CODE_OPTIONS[0].value,
      nationalNumber: ''
    };
  }

  const matchedCountryCode = [...PHONE_COUNTRY_CODE_OPTIONS]
    .sort((leftOption, rightOption) => rightOption.value.length - leftOption.value.length)
    .find((option) => sanitizedValue.startsWith(option.value));

  if (!matchedCountryCode) {
    return {
      countryCode: PHONE_COUNTRY_CODE_OPTIONS[0].value,
      nationalNumber: sanitizedValue
    };
  }

  return {
    countryCode: matchedCountryCode.value,
    nationalNumber: sanitizePhoneNationalNumber(sanitizedValue.slice(matchedCountryCode.value.length))
  };
}
