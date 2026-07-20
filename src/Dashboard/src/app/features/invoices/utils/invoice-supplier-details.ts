import { DashboardPersonDetails } from '../../persons/models/dashboard-person-details.model';
import { AddInvoiceSupplierDetails } from '../components/add-invoice-dialog/add-invoice-dialog.types';

const CONTACT_TYPES = {
  phone: 0,
  email: 1
} as const;

export function deriveInvoiceSupplierDetails(
  supplier: DashboardPersonDetails
): AddInvoiceSupplierDetails | null {
  if (supplier.eik === null || supplier.eik.trim().length === 0) {
    return null;
  }

  const address = pickSupplierAddress(supplier);
  const email = pickSupplierContactValue(supplier, CONTACT_TYPES.email);
  const phoneNumber = pickSupplierContactValue(supplier, CONTACT_TYPES.phone);

  if (!address || !email || !phoneNumber || supplier.displayName.trim().length === 0) {
    return null;
  }

  return {
    address,
    email,
    phoneNumber,
    contactPerson: supplier.displayName.trim()
  };
}

function pickSupplierAddress(supplier: DashboardPersonDetails): string | null {
  const address = [...supplier.addresses]
    .filter((item) => item.isActive)
    .sort((left, right) => Number(right.isPrimary) - Number(left.isPrimary))[0];

  if (!address) {
    return null;
  }

  const parts = [
    address.addressLine,
    address.additionalLine ?? '',
    address.city ?? '',
    address.postalCode ?? '',
    address.country ?? ''
  ]
    .map((part) => part.trim())
    .filter((part) => part.length > 0);

  return parts.length > 0 ? parts.join(', ') : null;
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
