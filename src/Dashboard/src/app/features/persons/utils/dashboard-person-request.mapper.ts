import {
  AddPersonAddressFormGroup,
  AddPersonBankAccountFormGroup,
  AddPersonContactFormGroup,
  AddPersonDialogFormGroup,
  CONTACT_TYPES,
  PERSON_TYPES
} from '../components/add-person-dialog/add-person-dialog.types';
import {
  CreateDashboardPersonAddressRequest,
  CreateDashboardPersonBankAccountRequest,
  CreateDashboardPersonContactRequest,
  CreateDashboardPersonRequest
} from '../models/create-dashboard-person-request.model';

const ADDRESS_ROW_DEFAULTS = {
  addressLine: '',
  additionalLine: '',
  city: '',
  postalCode: '',
  country: '',
  details: '',
  isPrimary: false,
  isActive: true
} as const;

const CONTACT_ROW_DEFAULTS = {
  contactType: CONTACT_TYPES.phone,
  value: '',
  details: '',
  isPrimary: false,
  isActive: true
} as const;

const BANK_ACCOUNT_ROW_DEFAULTS = {
  iban: '',
  bic: '',
  bankName: '',
  details: '',
  isPrimary: false,
  isActive: true
} as const;

type AddressRowValue = ReturnType<AddPersonAddressFormGroup['getRawValue']>;
type ContactRowValue = ReturnType<AddPersonContactFormGroup['getRawValue']>;
type BankAccountRowValue = ReturnType<AddPersonBankAccountFormGroup['getRawValue']>;

export function toCreateDashboardPersonRequest(
  personForm: AddPersonDialogFormGroup
): CreateDashboardPersonRequest {
  const formValue = personForm.getRawValue();
  const addresses = buildAddressRequests(formValue.addresses);
  const contacts = buildContactRequests(formValue.contacts);
  const bankAccounts = buildBankAccountRequests(formValue.bankAccounts);

  const request =
    formValue.type === PERSON_TYPES.individual
      ? {
          type: formValue.type,
          firstName: formValue.firstName,
          middleName: formValue.middleName,
          lastName: formValue.lastName,
          egn: formValue.egn,
          vatNumber: `${formValue.vatCountryCode}${formValue.egn}`
        }
      : {
          type: formValue.type,
          companyName: formValue.companyName,
          eik: formValue.eik,
          vatNumber: `${formValue.vatCountryCode}${formValue.eik}`
        };

  return {
    ...request,
    ...(addresses.length > 0 ? { addresses } : {}),
    ...(contacts.length > 0 ? { contacts } : {}),
    ...(bankAccounts.length > 0 ? { bankAccounts } : {})
  };
}

function buildAddressRequests(
  addresses: AddressRowValue[]
): CreateDashboardPersonAddressRequest[] {
  const requests: CreateDashboardPersonAddressRequest[] = [];

  for (const address of addresses) {
    if (!hasRepeatableContent(address, ADDRESS_ROW_DEFAULTS)) {
      continue;
    }

    requests.push({
      addressLine: address.addressLine,
      additionalLine: address.additionalLine,
      city: address.city,
      postalCode: address.postalCode,
      country: address.country,
      details: address.details,
      isPrimary: address.isPrimary,
      isActive: address.isActive
    });
  }

  return requests;
}

function buildContactRequests(
  contacts: ContactRowValue[]
): CreateDashboardPersonContactRequest[] {
  const requests: CreateDashboardPersonContactRequest[] = [];

  for (const contact of contacts) {
    if (!hasRepeatableContent(contact, CONTACT_ROW_DEFAULTS)) {
      continue;
    }

    requests.push({
      contactType: contact.contactType,
      value: contact.value,
      details: contact.details,
      isPrimary: contact.isPrimary,
      isActive: contact.isActive
    });
  }

  return requests;
}

function buildBankAccountRequests(
  bankAccounts: BankAccountRowValue[]
): CreateDashboardPersonBankAccountRequest[] {
  const requests: CreateDashboardPersonBankAccountRequest[] = [];

  for (const bankAccount of bankAccounts) {
    if (!hasRepeatableContent(bankAccount, BANK_ACCOUNT_ROW_DEFAULTS)) {
      continue;
    }

    requests.push({
      iban: bankAccount.iban,
      bic: bankAccount.bic,
      bankName: bankAccount.bankName,
      details: bankAccount.details,
      isPrimary: bankAccount.isPrimary,
      isActive: bankAccount.isActive
    });
  }

  return requests;
}

function hasRepeatableContent<T extends Record<string, unknown>>(
  value: T,
  defaults: Readonly<Record<string, unknown>>
): boolean {
  return Object.entries(defaults).some(([key, defaultValue]) => value[key] !== defaultValue);
}
