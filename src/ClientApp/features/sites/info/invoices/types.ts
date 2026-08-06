export type SiteInvoiceAllocation = {
  amount: number;
  direction: string;
};

export type SiteInvoice = {
  id: string;
  numberId: number;
  isComplete: boolean;
  supplierId: string | null;
  supplierDisplayLabel: string | null;
  submittedFromSiteName: string | null;
  invoiceNumber: string | null;
  date: string | null;
  created: string;
  taxIdentifier: string | null;
  address: string | null;
  email: string | null;
  phoneNumber: string | null;
  contactPerson: string | null;
  paymentTerm: string | null;
  totalValueExcludingVat: number | null;
  vatRate: number | null;
  vat: number | null;
  totalValueIncludingVat: number | null;
  paymentDate: string | null;
  paymentTime: string | null;
  paymentMethod: string | null;
  siteAllocation: SiteInvoiceAllocation | null;
};

export type InvoiceUploadAsset = {
  uri: string;
  fileName: string;
  contentType: string;
  fileSize?: number;
};

export type InvoiceFileAccess = {
  url: string;
  fileName: string;
  contentType: string;
  expiresAt: string;
};
