export type ProposalStatus = "Issued" | "Accepted" | "Rejected";

export type ClientProposalSummary = {
  id: string;
  numberId: number;
  revisionNumber: number;
  status: ProposalStatus;
  validUntil: string | null;
  issuedAt: string;
  firstViewedAt: string | null;
  respondedAt: string | null;
  responseComment: string | null;
  total: number;
  currencyCode: string;
  siteName: string;
  excludesUnpricedOptionalItems: boolean;
  canRespond: boolean;
  supersededByRevisionNumber: number | null;
};

export type ProposalActivitySection = {
  id: string;
  sourceOfferActivitySectionId: string;
  name: string | null;
  basisQuantity: number;
  requestedMeasurement: number;
  measurementUnit: string;
  pricingMode: string;
  priceAmount: number | null;
  calculatedTotal: number;
  sortOrder: number;
};

export type ProposalActivity = {
  id: string;
  sourceOfferActivityId: string;
  activityNumberId: number;
  name: string;
  description: string | null;
  sortOrder: number;
  sections: readonly ProposalActivitySection[];
};

export type ProposalProduct = {
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
  selectedPriceCurrencyCode: string | null;
  selectedPriceBasis: string | null;
  requiredTotal: number | null;
  optionalTotal: number | null;
  isOptionalPriceExcluded: boolean;
  sortOrder: number;
};

export type ClientProposalDetails = ClientProposalSummary & {
  siteNumberId: number;
  siteAddress: string;
  recipientDisplayName: string;
  recipientEmail: string;
  publicNotes: string | null;
  paymentTerms: string | null;
  activitySubtotalBeforeDiscount: number;
  activityDiscountPercentage: number;
  activityDiscountAmount: number;
  activityTotalAfterDiscount: number;
  productSubtotalBeforeDiscount: number;
  productDiscountPercentage: number;
  productDiscountAmount: number;
  productTotalAfterDiscount: number;
  unpricedOptionalItemCount: number;
  hasPdf: boolean;
  activities: readonly ProposalActivity[];
  products: readonly ProposalProduct[];
};

export type ProposalResponseDecision = "Accepted" | "Rejected";

export type ProposalPdfAccess = {
  url: string;
  fileName: string;
  contentType: string;
  expiresAt: string;
};
