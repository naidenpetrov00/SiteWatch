import { DashboardPersonDetails } from '../../persons/models/dashboard-person-details.model';
import { InvoiceSupplierValidationResult } from '../components/add-invoice-dialog/add-invoice-dialog.types';

const CONTACT_TYPES = {
  phone: 0,
  email: 1
} as const;

export function deriveInvoiceSupplierDetails(
  supplier: DashboardPersonDetails
): InvoiceSupplierValidationResult {
  const identifier = supplier.type.toLowerCase() === 'company' ? supplier.eik : supplier.egn;
  if (identifier === null || identifier.trim().length === 0) {
    return {
      details: null,
      error: supplier.type.toLowerCase() === 'company'
        ? 'Company is missing EIK.'
        : 'Individual is missing EGN.'
    };
  }

  const address = pickSupplierAddress(supplier);
  const email = pickSupplierContactValue(supplier, CONTACT_TYPES.email);
  const phoneNumber = pickSupplierContactValue(supplier, CONTACT_TYPES.phone);
  const contactPerson = supplier.displayName.trim();
  const iban = pickSupplierIban(supplier);

  if (!address) {
    return { details: null, error: 'Supplier is missing an active address.' };
  }

  if (contactPerson.length === 0) {
    return { details: null, error: 'Supplier is missing a display name.' };
  }

  return {
    details: {
      address,
      email: email ?? '',
      phoneNumber: phoneNumber ?? '',
      contactPerson,
      iban
    },
    error: null
  };
}

function pickSupplierIban(supplier: DashboardPersonDetails): string {
  return [...supplier.bankAccounts]
    .filter((item) => item.isActive && item.iban.trim().length > 0)
    .sort((left, right) => Number(right.isPrimary) - Number(left.isPrimary))
    .map((item) => item.iban.trim())
    .at(0) ?? '';
}

function pickSupplierAddress(supplier: DashboardPersonDetails): string | null {
  const addresses = [...supplier.addresses]
    .filter((item) => item.isActive)
    .sort((left, right) => Number(right.isPrimary) - Number(left.isPrimary));

  for (const address of addresses) {
    const parts = [
      address.addressLine,
      address.additionalLine ?? '',
      address.city ?? '',
      address.postalCode ?? '',
      address.country ?? ''
    ]
      .map((part) => part.trim())
      .filter((part) => part.length > 0);

    if (parts.length > 0) {
      return parts.join(', ');
    }
  }

  return null;
}

function pickSupplierContactValue(
  supplier: DashboardPersonDetails,
  contactType: number
): string | null {
  const contact = [...supplier.contacts]
    .filter((item) => item.isActive && item.contactType === contactType)
    .sort((left, right) => Number(right.isPrimary) - Number(left.isPrimary))
    .find((item) => item.value.trim().length > 0);

  return contact ? contact.value.trim() : null;
}
