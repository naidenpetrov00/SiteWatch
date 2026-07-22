export const SITE_PAYMENT_DIRECTIONS = ['In', 'Out'] as const;

export type SitePaymentDirection = (typeof SITE_PAYMENT_DIRECTIONS)[number];

export interface InvoiceSiteAllocationRequest {
  siteId: string;
  amount: number;
  direction: SitePaymentDirection;
}

export interface DashboardInvoiceSiteAllocation extends InvoiceSiteAllocationRequest {
  siteNumberId: number;
  siteName: string;
}

export interface UpdateInvoiceSiteAllocationsRequest {
  invoiceId: string;
  siteAllocations: readonly InvoiceSiteAllocationRequest[];
}
