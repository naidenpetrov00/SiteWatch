import { FormArray, FormControl, FormGroup } from '@angular/forms';

import { DialogWizardTabDefinition } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.types';

export const PERSON_TYPES = {
  individual: 0,
  company: 1
} as const;

export const CONTACT_TYPES = {
  phone: 0,
  email: 1,
  website: 2,
  other: 3
} as const;

export const LEGAL_FORM_OPTIONS = [
  { value: 'ООД', label: 'ООД' },
  { value: 'ЕООД', label: 'ЕООД' },
  { value: 'АД', label: 'АД' },
  { value: 'ЕАД', label: 'ЕАД' },
  { value: 'ЕТ', label: 'ЕТ' },
  { value: 'СД', label: 'СД' },
  { value: 'КД', label: 'КД' },
  { value: 'КДА', label: 'КДА' },
  { value: 'ДЗЗД', label: 'ДЗЗД' }
] as const satisfies readonly {
  value: string;
  label: string;
}[];

export type AddPersonTypeOption = (typeof PERSON_TYPES)[keyof typeof PERSON_TYPES];
export type AddPersonContactTypeOption = (typeof CONTACT_TYPES)[keyof typeof CONTACT_TYPES];
export type AddPersonLegalFormOption = (typeof LEGAL_FORM_OPTIONS)[number]['value'];
export type AddPersonVatCountryCodeOption = 'BG';
export type AddPersonPhoneCountryCodeOption = '359' | '1' | '44' | '49';
export type AddPersonWizardTabId = 'primary-info' | 'addresses' | 'contacts' | 'bank-accounts';

export interface AddPersonAddressFormControls {
  addressLine: FormControl<string>;
  additionalLine: FormControl<string>;
  city: FormControl<string>;
  postalCode: FormControl<string>;
  country: FormControl<string>;
  details: FormControl<string>;
  isPrimary: FormControl<boolean>;
  isActive: FormControl<boolean>;
}

export interface AddPersonContactFormControls {
  contactType: FormControl<AddPersonContactTypeOption>;
  value: FormControl<string>;
  details: FormControl<string>;
  isPrimary: FormControl<boolean>;
  isActive: FormControl<boolean>;
}

export interface AddPersonBankAccountFormControls {
  iban: FormControl<string>;
  bic: FormControl<string>;
  bankName: FormControl<string>;
  details: FormControl<string>;
  isPrimary: FormControl<boolean>;
  isActive: FormControl<boolean>;
}

export interface AddPersonDialogFormControls {
  type: FormControl<AddPersonTypeOption>;
  firstName: FormControl<string>;
  middleName: FormControl<string>;
  lastName: FormControl<string>;
  companyName: FormControl<string>;
  legalForm: FormControl<AddPersonLegalFormOption | ''>;
  egn: FormControl<string>;
  eik: FormControl<string>;
  vatCountryCode: FormControl<AddPersonVatCountryCodeOption>;
  vatNumber: FormControl<string>;
  addresses: AddPersonAddressesFormArray;
  contacts: AddPersonContactsFormArray;
  bankAccounts: AddPersonBankAccountsFormArray;
}

export type AddPersonAddressFormGroup = FormGroup<AddPersonAddressFormControls>;
export type AddPersonContactFormGroup = FormGroup<AddPersonContactFormControls>;
export type AddPersonBankAccountFormGroup = FormGroup<AddPersonBankAccountFormControls>;
export type AddPersonDialogFormGroup = FormGroup<AddPersonDialogFormControls>;

export type AddPersonAddressesFormArray = FormArray<AddPersonAddressFormGroup>;
export type AddPersonContactsFormArray = FormArray<AddPersonContactFormGroup>;
export type AddPersonBankAccountsFormArray = FormArray<AddPersonBankAccountFormGroup>;

export const PERSON_TYPE_OPTIONS = [
  { value: PERSON_TYPES.individual, label: 'Individual' },
  { value: PERSON_TYPES.company, label: 'Company' }
] as const satisfies readonly {
  value: AddPersonTypeOption;
  label: string;
}[];

export const CONTACT_TYPE_OPTIONS = [
  { value: CONTACT_TYPES.phone, label: 'Phone' },
  { value: CONTACT_TYPES.email, label: 'Email' },
  { value: CONTACT_TYPES.website, label: 'Website' },
  { value: CONTACT_TYPES.other, label: 'Other' }
] as const satisfies readonly {
  value: AddPersonContactTypeOption;
  label: string;
}[];

export const VAT_COUNTRY_CODE_OPTIONS = [{ value: 'BG', label: 'BG' }] as const satisfies readonly {
  value: AddPersonVatCountryCodeOption;
  label: string;
}[];

export const PHONE_COUNTRY_CODE_OPTIONS = [
  { value: '359', label: 'BG (+359)' },
  { value: '1', label: 'US (+1)' },
  { value: '44', label: 'UK (+44)' },
  { value: '49', label: 'DE (+49)' }
] as const satisfies readonly {
  value: AddPersonPhoneCountryCodeOption;
  label: string;
}[];

export const ADD_PERSON_WIZARD_TABS = [
  {
    id: 'primary-info',
    label: 'Primary Info'
  },
  {
    id: 'addresses',
    label: 'Addresses'
  },
  {
    id: 'contacts',
    label: 'Contacts'
  },
  {
    id: 'bank-accounts',
    label: 'Bank Accounts'
  }
] as const satisfies readonly DialogWizardTabDefinition[];
