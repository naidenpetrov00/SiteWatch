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
  currencyCode: "EUR";
  siteName: string;
  excludesUnpricedOptionalItems: boolean;
  canRespond: boolean;
  supersededByRevisionNumber: number | null;
};

export type ProposalActivitySection = {
  id: string;
  name: string | null;
  requestedMeasurement: number;
  measurementUnit: string;
  pricingMode: string;
  priceAmount: number | null;
  calculatedTotal: number;
};

export type ProposalActivity = {
  id: string;
  activityNumberId: number;
  name: string;
  description: string | null;
  sections: readonly ProposalActivitySection[];
};

export type ProposalProduct = {
  id: string;
  productNumberId: number;
  title: string;
  brand: string | null;
  model: string | null;
  requiredQuantity: number;
  optionalQuantity: number;
  selectedRetailerDisplayName: string | null;
  selectedPriceAmount: number | null;
  selectedPriceBasis: string | null;
  requiredTotal: number | null;
  optionalTotal: number | null;
  isOptionalPriceExcluded: boolean;
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
