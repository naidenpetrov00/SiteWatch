export interface CreateDashboardInvoiceRequest {
  supplierId: string;
  invoiceNumber: string;
  date: string;
  iban: string;
  paymentTerm: string;
  totalValue: number;
  vatRate: number;
  paymentMethod: string;
  paymentDate?: string;
  paymentTime?: string;
}
