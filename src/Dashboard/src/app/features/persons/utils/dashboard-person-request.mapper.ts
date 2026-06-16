import { AddPersonDialogFormGroup } from '../components/add-person-dialog/add-person-dialog.types';
import {
  CreateDashboardPersonAddressRequest,
  CreateDashboardPersonBankAccountRequest,
  CreateDashboardPersonContactRequest,
  CreateDashboardPersonRequest,
  CreateDashboardPersonType
} from '../models/create-dashboard-person-request.model';

const PERSON_TYPE_MAP: Record<'individual' | 'company', CreateDashboardPersonType> = {
  individual: 0,
  company: 1
};

const CONTACT_TYPE_MAP: Record<'phone' | 'email' | 'website' | 'other', 0 | 1 | 2 | 3> = {
  phone: 0,
  email: 1,
  website: 2,
  other: 3
};

export function toCreateDashboardPersonRequest(
  personForm: AddPersonDialogFormGroup
): CreateDashboardPersonRequest | null {
  const formValue = personForm.getRawValue();
  const type = PERSON_TYPE_MAP[formValue.type];
  const addresses = buildAddressRequests(personForm);
  const contacts = buildContactRequests(personForm);
  const bankAccounts = buildBankAccountRequests(personForm);

  if (addresses === null || contacts === null || bankAccounts === null) {
    return null;
  }

  if (formValue.type === 'individual') {
    const firstName = trimRequired(formValue.firstName);
    const lastName = trimRequired(formValue.lastName);
    const egn = normalizeDigits(formValue.egn);

    if (firstName === null || lastName === null || egn === null) {
      return null;
    }

    const request: CreateDashboardPersonRequest = {
      type,
      firstName,
      middleName: trimOptional(formValue.middleName),
      lastName,
      egn,
      vatNumber: `${formValue.vatCountryCode}${egn}`
    };

    return attachChildCollections(request, addresses, contacts, bankAccounts);
  }

  const companyName = trimRequired(formValue.companyName);
  const eik = normalizeDigits(formValue.eik);
  const vatCountryCode = trimRequired(formValue.vatCountryCode);
  const vatNumber = normalizeDigits(formValue.vatNumber);

  if (
    companyName === null ||
    eik === null ||
    vatCountryCode === null ||
    vatNumber === null
  ) {
    return null;
  }

  const request: CreateDashboardPersonRequest = {
    type,
    companyName,
    eik,
    vatNumber: `${vatCountryCode}${vatNumber}`
  };

  return attachChildCollections(request, addresses, contacts, bankAccounts);
}

function attachChildCollections(
  request: CreateDashboardPersonRequest,
  addresses: CreateDashboardPersonAddressRequest[],
  contacts: CreateDashboardPersonContactRequest[],
  bankAccounts: CreateDashboardPersonBankAccountRequest[]
): CreateDashboardPersonRequest {
  return {
    ...request,
    ...(addresses.length > 0 ? { addresses } : {}),
    ...(contacts.length > 0 ? { contacts } : {}),
    ...(bankAccounts.length > 0 ? { bankAccounts } : {})
  };
}

function buildAddressRequests(
  personForm: AddPersonDialogFormGroup
): CreateDashboardPersonAddressRequest[] | null {
  const requests: CreateDashboardPersonAddressRequest[] = [];

  for (const addressGroup of personForm.controls.addresses.controls) {
    const address = addressGroup.getRawValue();
    const normalizedAddressLine = trimRequired(address.addressLine);
    const hasAnyContent =
      normalizedAddressLine !== null ||
      hasText(address.additionalLine) ||
      hasText(address.city) ||
      hasText(address.postalCode) ||
      hasText(address.country) ||
      hasText(address.details) ||
      address.isPrimary ||
      !address.isActive;

    if (!hasAnyContent) {
      continue;
    }

    if (normalizedAddressLine === null) {
      return null;
    }

    requests.push({
      addressLine: normalizedAddressLine,
      additionalLine: trimOptional(address.additionalLine),
      city: trimOptional(address.city),
      postalCode: trimOptional(address.postalCode),
      country: trimOptional(address.country),
      details: trimOptional(address.details),
      isPrimary: address.isPrimary,
      isActive: address.isActive
    });
  }

  return requests;
}

function buildContactRequests(
  personForm: AddPersonDialogFormGroup
): CreateDashboardPersonContactRequest[] | null {
  const requests: CreateDashboardPersonContactRequest[] = [];

  for (const contactGroup of personForm.controls.contacts.controls) {
    const contact = contactGroup.getRawValue();
    const normalizedValue = trimRequired(contact.value);
    const hasAnyContent =
      normalizedValue !== null ||
      hasText(contact.details) ||
      contact.contactType !== 'phone' ||
      contact.isPrimary ||
      !contact.isActive;

    if (!hasAnyContent) {
      continue;
    }

    if (normalizedValue === null) {
      return null;
    }

    requests.push({
      contactType: CONTACT_TYPE_MAP[contact.contactType],
      value: normalizedValue,
      details: trimOptional(contact.details),
      isPrimary: contact.isPrimary,
      isActive: contact.isActive
    });
  }

  return requests;
}

function buildBankAccountRequests(
  personForm: AddPersonDialogFormGroup
): CreateDashboardPersonBankAccountRequest[] | null {
  const requests: CreateDashboardPersonBankAccountRequest[] = [];

  for (const bankAccountGroup of personForm.controls.bankAccounts.controls) {
    const bankAccount = bankAccountGroup.getRawValue();
    const normalizedIban = trimRequired(bankAccount.iban);
    const hasAnyContent =
      normalizedIban !== null ||
      hasText(bankAccount.bic) ||
      hasText(bankAccount.bankName) ||
      hasText(bankAccount.details) ||
      bankAccount.isPrimary ||
      !bankAccount.isActive;

    if (!hasAnyContent) {
      continue;
    }

    if (normalizedIban === null) {
      return null;
    }

    requests.push({
      iban: normalizedIban,
      bic: trimOptional(bankAccount.bic),
      bankName: trimOptional(bankAccount.bankName),
      details: trimOptional(bankAccount.details),
      isPrimary: bankAccount.isPrimary,
      isActive: bankAccount.isActive
    });
  }

  return requests;
}

function trimRequired(value: string): string | null {
  const normalizedValue = value.trim();

  return normalizedValue.length > 0 ? normalizedValue : null;
}

function trimOptional(value: string): string | undefined {
  const normalizedValue = value.trim();

  return normalizedValue.length > 0 ? normalizedValue : undefined;
}

function hasText(value: string): boolean {
  return value.trim().length > 0;
}

function normalizeDigits(value: string): string | null {
  const normalizedValue = value.replace(/\D+/g, '');

  return normalizedValue.length > 0 ? normalizedValue : null;
}
