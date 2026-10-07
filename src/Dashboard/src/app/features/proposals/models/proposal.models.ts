export type ProposalStatus = 'Draft' | 'Issued';

export interface ProposalSummary {
  id: string;
  numberId: number;
  revisionNumber: number;
  siteId: string;
  sourceOfferId: string;
  sourceOfferNumberId: number;
  recipientDisplayName: string;
  recipientEmail: string;
  status: ProposalStatus;
  validUntil: string | null;
  created: string;
  issuedAt: string | null;
  total: number;
  currencyCode: 'EUR';
  excludesUnpricedOptionalItems: boolean;
  unpricedOptionalItemCount: number;
}

export interface ProposalActivitySection {
  id: string;
  sourceOfferActivitySectionId: string;
  name: string | null;
  basisQuantity: number;
  measurementUnit: string;
  requestedMeasurement: number;
  pricingMode: string;
  priceAmount: number | null;
  calculatedTotal: number;
  sortOrder: number;
}

export interface ProposalActivity {
  id: string;
  sourceOfferActivityId: string;
  activityNumberId: number;
  name: string;
  description: string | null;
  sortOrder: number;
  sections: readonly ProposalActivitySection[];
}

export interface ProposalProductLine {
  id: string;
  sourceOfferProductLineId: string;
  productId: string;
  productNumberId: number;
  title: string;
  category: string;
  brand: string | null;
  model: string | null;
  packageQuantity: number | null;
  packageUnit: string | null;
  requiredQuantity: number;
  optionalQuantity: number;
  selectedRetailerId: string | null;
  selectedRetailerDisplayName: string | null;
  selectedPriceAmount: number | null;
  selectedPriceCurrencyCode: 'EUR' | null;
  selectedPriceBasis: string | null;
  requiredTotal: number | null;
  optionalTotal: number | null;
  isOptionalPriceExcluded: boolean;
  sortOrder: number;
}

export interface ProposalDetails extends ProposalSummary {
  siteNumberId: number;
  siteName: string;
  siteAddress: string;
  sourceOfferFinalizedAt: string;
  recipientUserId: string;
  publicNotes: string | null;
  paymentTerms: string | null;
  lastModified: string;
  issuedBy: string | null;
  activitySubtotalBeforeDiscount: number;
  activityDiscountPercentage: number;
  activityDiscountAmount: number;
  activityTotalAfterDiscount: number;
  productSubtotalBeforeDiscount: number;
  productDiscountPercentage: number;
  productDiscountAmount: number;
  productTotalAfterDiscount: number;
  hasPdf: boolean;
  activities: readonly ProposalActivity[];
  products: readonly ProposalProductLine[];
}

export interface CreateProposalResponse {
  id: string;
}

export interface UpdateProposalMetadataRequest {
  siteId: string;
  proposalId: string;
  validUntil: string | null;
  publicNotes: string | null;
  paymentTerms: string | null;
}

export interface ProposalPdfAccess {
  url: string;
  fileName: string;
  contentType: string;
  expiresAt: string;
}
