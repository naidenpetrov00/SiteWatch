export type SiteInvoiceAllocation = {
  amount: number;
  direction: string;
};

export type SiteInvoice = {
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
  paymentTerm: string;
  totalValueExcludingVat: number;
  vat: number;
  totalValueIncludingVat: number;
  paymentDate: string | null;
  paymentTime: string | null;
  paymentMethod: string;
  siteAllocation: SiteInvoiceAllocation;
};

export type InvoiceFileAccess = {
  url: string;
  fileName: string;
  contentType: string;
  expiresAt: string;
};
