export interface DashboardInvoice {
  id: string;
  supplierId: string;
  supplierDisplayLabel: string;
  invoiceNumber: string;
  date: string;
  eik: string;
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
