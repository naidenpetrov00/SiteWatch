export interface DashboardInvoice {
  id: string;
  numberId: number;
  supplierId: string;
  supplierDisplayLabel: string;
  invoiceNumber: string;
  date: string;
  taxIdentifier: string;
  address: string;
  email: string;
  phoneNumber: string;
  contactPerson: string;
  iban: string;
  paymentTerm: string;
  totalValueExcludingVat: number;
  vat: number;
  totalValueIncludingVat: number;
  paymentDate: string | null;
  paymentTime: string | null;
  paymentMethod: string;
}
