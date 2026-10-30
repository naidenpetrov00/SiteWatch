export type AssetClassification = "Machine" | "Tool";
export type AssetReviewStatus = "PendingReview" | "Active" | "Rejected";

export interface PagedResponse<T> {
  items: T[];
  filteredCount: number;
  totalCount: number;
}

export interface MobileAssetListItem {
  id: string;
  numberId: number;
  assetCode: string;
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  manufacturerSerialNumber: string | null;
  siteId: string;
  siteName: string;
  condition: string;
  operationalStatus: string;
}

export interface MobileAssetDetails extends MobileAssetListItem {
  qrIdentifier: string;
  description: string | null;
  responsibleAssigneeDisplayName: string | null;
}

export interface MobileAssetRegistration {
  id: string;
  numberId: number;
  assetCode: string;
  qrIdentifier: string;
  reviewStatus: AssetReviewStatus;
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  description: string | null;
  manufacturerSerialNumber: string | null;
  siteId: string;
  siteName: string;
  isAssignedToCurrentUser: boolean;
  condition: string;
  operationalStatus: string;
}

export interface MobileAssetSite {
  id: string;
  name: string;
  address: string;
  isCompanyStock: boolean;
}

export interface RegisterMobileAssetRequest {
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  description: string | null;
  manufacturerSerialNumber: string | null;
  siteId: string;
  assignToMe: boolean;
}
