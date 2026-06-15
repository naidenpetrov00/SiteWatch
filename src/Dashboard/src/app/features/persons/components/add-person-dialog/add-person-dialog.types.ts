import { FormArray, FormControl, FormGroup } from '@angular/forms';

import { DialogWizardTabDefinition } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.types';

export type AddPersonTypeOption = 'individual' | 'company';
export type AddPersonContactTypeOption = 'phone' | 'email' | 'website' | 'other';
export type AddPersonVatCountryCodeOption = 'BG';
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
  { value: 'individual', label: 'Individual' },
  { value: 'company', label: 'Company' }
] as const satisfies readonly {
  value: AddPersonTypeOption;
  label: string;
}[];

export const CONTACT_TYPE_OPTIONS = [
  { value: 'phone', label: 'Phone' },
  { value: 'email', label: 'Email' },
  { value: 'website', label: 'Website' },
  { value: 'other', label: 'Other' }
] as const satisfies readonly {
  value: AddPersonContactTypeOption;
  label: string;
}[];

export const VAT_COUNTRY_CODE_OPTIONS = [{ value: 'BG', label: 'BG' }] as const satisfies readonly {
  value: AddPersonVatCountryCodeOption;
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
