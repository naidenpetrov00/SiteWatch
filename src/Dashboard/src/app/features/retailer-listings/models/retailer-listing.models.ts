import { DashboardProductDetails, DashboardProductLookup } from '../../products/models/dashboard-product.models';
import { DashboardRetailerDetails, DashboardRetailerLookup } from '../../retailers/models/dashboard-retailer.models';

export type RetailerPriceBasis = 'item' | 'package';
export type RetailerPriceSource = 'manual' | 'automated';

export interface RetailerPriceObservation {
  id: string;
  amount: number;
  currencyCode: 'EUR';
  basis: RetailerPriceBasis;
  observedAt: string;
  recordedAt: string;
  source: RetailerPriceSource;
  sourceReference: string | null;
  recordedBy: string;
}

export interface RetailerListing {
  id: string;
  productId: string;
  productNumberId: number;
  productTitle: string;
  productStatus: string;
  productBrand: string | null;
  productModel: string | null;
  productPackageQuantity: number | null;
  productPackageUnit: string | null;
  retailerId: string;
  retailerDisplayName: string;
  retailerBaseWebsiteUrl: string;
  retailerIsActive: boolean;
  isActive: boolean;
  productUrl: string | null;
  retailerProductCode: string | null;
  created: string;
  lastModified: string;
  latestObservation: RetailerPriceObservation | null;
}

export interface RetailerListingsResponse {
  items: readonly RetailerListing[];
  filteredCount: number;
  totalCount: number;
}

export interface RetailerPriceHistory {
  productId: string;
  retailerId: string;
  retailerListingId: string | null;
  retailerListingIsActive: boolean | null;
  productUrl: string | null;
  retailerProductCode: string | null;
  items: readonly RetailerPriceObservation[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
}

export interface RetailerListingQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  searchTerm: string;
  includeInactive: boolean;
  isActive: boolean | null;
}

export interface SaveRetailerListingRequest {
  productId: string;
  retailerId: string;
  productUrl: string | null;
  retailerProductCode: string | null;
  amount: number | null;
  basis: RetailerPriceBasis | null;
}

export interface UpdateRetailerListingRequest {
  listingId: string;
  productUrl: string | null;
  retailerProductCode: string | null;
  amount: number | null;
  basis: RetailerPriceBasis | null;
}

export type RetailerListingFixedProduct =
  | DashboardProductDetails
  | DashboardProductLookup;
export type RetailerListingFixedRetailer =
  | DashboardRetailerDetails
  | DashboardRetailerLookup;

export interface RetailerListingDialogData {
  listing: RetailerListing | null;
  fixedProduct?: RetailerListingFixedProduct;
  fixedRetailer?: RetailerListingFixedRetailer;
}

export interface RetailerPriceHistoryDialogData {
  productId: string;
  productLabel: string;
  retailerId: string;
  retailerName: string;
}
