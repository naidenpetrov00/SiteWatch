export type RetailerPriceCollectionRunStatus =
  | 'queued'
  | 'running'
  | 'completed'
  | 'completedWithFailures';

export type RetailerPriceCollectionRunItemStatus =
  | 'queued'
  | 'running'
  | 'succeeded'
  | 'failed'
  | 'skipped';

export interface RetailerPriceCollectionRunSummary {
  id: string;
  companyPersonId: string;
  companyDisplayName: string;
  extractionProfileId: string;
  extractionProfileVersion: number;
  status: RetailerPriceCollectionRunStatus;
  requestedBy: string;
  requestedAt: string;
  startedAt: string | null;
  completedAt: string | null;
  totalCount: number;
  queuedCount: number;
  runningCount: number;
  processedCount: number;
  succeededCount: number;
  failedCount: number;
  skippedCount: number;
}

export interface RetailerPriceCollectionRunItem {
  id: string;
  retailerListingId: string;
  retailerId: string;
  retailerDisplayName: string;
  productId: string;
  productNumberId: number;
  productTitle: string;
  retailerProductCode: string | null;
  status: RetailerPriceCollectionRunItemStatus;
  capturedProductUrl: string | null;
  finalSourceUrl: string | null;
  startedAt: string | null;
  completedAt: string | null;
  amount: number | null;
  currencyCode: 'EUR' | null;
  priceBasis: 'item' | 'package' | null;
  observedAt: string | null;
  recordedAt: string | null;
  matchedRuleId: string | null;
  matchedRuleName: string | null;
  matchedRulePriority: number | null;
  retailerPriceObservationId: string | null;
  diagnosticCode: string | null;
  diagnosticMessage: string | null;
}

export interface RetailerPriceCollectionRunDetails {
  run: RetailerPriceCollectionRunSummary;
  items: readonly RetailerPriceCollectionRunItem[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
}

export function isPriceCollectionRunUnfinished(
  run: RetailerPriceCollectionRunSummary
): boolean {
  return run.status === 'queued' || run.status === 'running';
}
