export interface OfferDefaultRetailer {
  retailerId: string;
  displayName: string;
  baseWebsiteUrl: string;
  websiteHost: string;
}

export interface ReplaceOfferDefaultRetailersRequest {
  retailerIds: readonly string[];
}
