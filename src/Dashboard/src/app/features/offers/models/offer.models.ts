export type OfferStatus = 'Draft' | 'Finalized' | 'Archived';

export interface OfferSummary {
  id: string;
  numberId: number;
  siteId: string;
  title: string | null;
  status: OfferStatus;
  created: string;
  lastModified: string;
}

export interface OfferDetails extends OfferSummary {
  siteNumberId: number;
  siteName: string;
  siteAddress: string;
  notes: string | null;
  createdBy: string | null;
  lastModifiedBy: string | null;
  activities: readonly OfferActivity[];
  products: readonly OfferProductLine[];
}

export type OfferActivityMeasurementUnit = 'piece' | 'cm' | 'm' | 'm2' | 'm3';
export type OfferProductQuantityBehavior = 'proportional' | 'fixed';

export interface OfferActivity {
  id: string;
  sourceActivityId: string;
  activityNumberId: number;
  name: string;
  description: string | null;
  sortOrder: number;
  sections: readonly OfferActivitySection[];
}

export interface OfferActivitySection {
  id: string;
  sourceSectionId: string;
  name: string | null;
  basisQuantity: number;
  measurementUnit: OfferActivityMeasurementUnit;
  requestedMeasurement: number;
  sortOrder: number;
}

export interface OfferProductLine {
  id: string;
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
  contributions: readonly OfferProductContribution[];
}

export interface OfferProductContribution {
  id: string;
  sourceRequirementId: string;
  offerActivityId: string;
  activityNumberId: number;
  activityName: string;
  offerActivitySectionId: string;
  sourceSectionId: string;
  sectionName: string | null;
  basisQuantity: number;
  measurementUnit: OfferActivityMeasurementUnit;
  requestedMeasurement: number;
  configuredQuantity: number;
  isRequired: boolean;
  quantityBehavior: OfferProductQuantityBehavior;
  notes: string | null;
  sortOrder: number;
  calculatedQuantity: number;
}

export interface OfferActivityCatalogNode {
  id: string;
  kind: 'folder' | 'activity';
  parentFolderId: string | null;
  name: string;
  sortOrder: number;
  numberId: number | null;
  status: 'Active' | 'Archived' | null;
  isSelected: boolean;
}

export type OfferActivityCatalogTreeNode = OfferActivityCatalogNode & {
  children: OfferActivityCatalogTreeNode[];
};

export interface OfferActivityCandidateSection {
  id: string;
  name: string | null;
  basisQuantity: number;
  measurementUnit: OfferActivityMeasurementUnit;
  sortOrder: number;
}

export interface OfferActivityCandidate {
  id: string;
  numberId: number;
  name: string;
  description: string | null;
  sections: readonly OfferActivityCandidateSection[];
}

export interface OfferSectionMeasurementRequest {
  sectionId: string;
  requestedMeasurement: number;
}

export interface AddOfferActivityRequest {
  siteId: string;
  offerId: string;
  activityId: string;
  sectionMeasurements: readonly OfferSectionMeasurementRequest[];
}

export interface RemoveOfferActivityRequest {
  siteId: string;
  offerId: string;
  offerActivityId: string;
}

export interface UpdateOfferActivityMeasurementsRequest
  extends RemoveOfferActivityRequest {
  sectionMeasurements: readonly OfferSectionMeasurementRequest[];
}

export interface OfferSiteIdentity {
  id: string;
  numberId: number;
  name: string;
  address: string;
}

export interface SiteOffersResponse {
  items: readonly OfferSummary[];
  filteredCount: number;
  totalCount: number;
}

export interface CreateOfferResponse {
  id: string;
}

export interface UpdateOfferMetadataRequest {
  siteId: string;
  offerId: string;
  title: string | null;
  notes: string | null;
}

export const OFFER_STATUS_OPTIONS = [
  { label: 'Draft', value: 'Draft' },
  { label: 'Finalized', value: 'Finalized' },
  { label: 'Archived', value: 'Archived' }
] as const;

export type OfferPriceBasis = 'item' | 'package';
export type OfferPriceSource = 'manual' | 'automated';

export interface OfferPriceObservation {
  id: string;
  amount: number;
  currencyCode: 'EUR';
  basis: OfferPriceBasis;
  observedAt: string;
  recordedAt: string;
  source: OfferPriceSource;
  sourceReference: string | null;
  recordedBy: string;
}

export interface OfferSelectedPrice {
  retailerId: string;
  retailerListingId: string;
  retailerPriceObservationId: string;
  retailerDisplayName: string;
  productUrl: string | null;
  retailerProductCode: string | null;
  amount: number;
  currencyCode: 'EUR';
  basis: OfferPriceBasis;
  observedAt: string;
  recordedAt: string;
  source: OfferPriceSource;
  sourceReference: string | null;
  recordedBy: string;
  selectedAt: string;
  selectedBy: string;
  hasNewerObservation: boolean;
}

export interface OfferPricingRetailer {
  retailerId: string;
  displayName: string;
  baseWebsiteUrl: string;
  isActive: boolean;
}

export interface OfferRetailerPriceCell {
  offerProductLineId: string;
  productId: string;
  retailerId: string;
  retailerListingId: string | null;
  productUrl: string | null;
  retailerProductCode: string | null;
  latestObservation: OfferPriceObservation | null;
  comparableRequiredTotal: number | null;
  comparableOptionalTotal: number | null;
  isCheapest: boolean;
}

export interface OfferPricingRequirement {
  id: string;
  offerActivityId: string;
  activityNumberId: number;
  activityName: string;
  activitySortOrder: number;
  offerActivitySectionId: string;
  sectionName: string | null;
  sectionSortOrder: number;
  basisQuantity: number;
  measurementUnit: OfferActivityMeasurementUnit;
  requestedMeasurement: number;
  configuredQuantity: number;
  isRequired: boolean;
  quantityBehavior: OfferProductQuantityBehavior;
  notes: string | null;
  sortOrder: number;
  calculatedQuantity: number;
}

export interface OfferPricingProductRow {
  offerProductLineId: string;
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
  requirements: readonly OfferPricingRequirement[];
  selectedRequiredTotal: number | null;
  selectedOptionalTotal: number | null;
  selectedPrice: OfferSelectedPrice | null;
  retailerPrices: readonly OfferRetailerPriceCell[];
}

export interface OfferPricingMatrix {
  offerId: string;
  status: OfferStatus;
  currencyCode: 'EUR';
  requiredPricingComplete: boolean;
  optionalPricingComplete: boolean;
  requiredTotal: number | null;
  optionalTotal: number | null;
  retailers: readonly OfferPricingRetailer[];
  products: readonly OfferPricingProductRow[];
}

export interface RetailerPriceHistory {
  productId: string;
  retailerId: string;
  retailerListingId: string | null;
  productUrl: string | null;
  retailerProductCode: string | null;
  items: readonly OfferPriceObservation[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
}

export interface AddOfferPricingRetailerRequest {
  siteId: string;
  offerId: string;
  retailerId: string;
}

export interface RecordManualOfferPriceRequest {
  siteId: string;
  offerId: string;
  offerProductLineId: string;
  productId: string;
  retailerId: string;
  amount: number;
  basis: OfferPriceBasis;
  productUrl: string | null;
  retailerProductCode: string | null;
}

export interface SelectOfferProductPriceRequest {
  siteId: string;
  offerId: string;
  offerProductLineId: string;
  retailerId: string;
  observationId: string;
}
