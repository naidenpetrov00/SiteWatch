export type AssetClassification = 'Machine' | 'Tool';
export type AssetReviewStatus = 'PendingReview' | 'Active' | 'Rejected';
export type AssetCondition = 'Good' | 'Damaged' | 'Unsafe';
export type AssetOperationalStatus =
  | 'Available'
  | 'InUse'
  | 'InTransfer'
  | 'Missing'
  | 'UnderRepair'
  | 'Retired';

export const ASSET_CLASSIFICATIONS: readonly AssetClassification[] = ['Machine', 'Tool'];
export const ASSET_CONDITIONS: readonly AssetCondition[] = ['Good', 'Damaged', 'Unsafe'];
export const REGISTRATION_STATUSES: readonly AssetOperationalStatus[] = [
  'Available',
  'InUse',
  'UnderRepair'
];
export const REVIEW_STATUSES: readonly AssetReviewStatus[] = [
  'PendingReview',
  'Active',
  'Rejected'
];

export interface PagedResponse<T> {
  items: readonly T[];
  filteredCount: number;
  totalCount: number;
}

export interface AssetListItem {
  id: string;
  numberId: number;
  assetCode: string;
  qrIdentifier: string;
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  manufacturerSerialNumber: string | null;
  siteId: string;
  siteName: string;
  responsibleUserId: string | null;
  responsibleUserDisplayName: string | null;
  condition: AssetCondition;
  operationalStatus: AssetOperationalStatus;
  reviewStatus: AssetReviewStatus;
  hasPendingHandover: boolean;
}

export interface AssetDetails extends AssetListItem {
  description: string | null;
  pendingHandoverRecipientId: string | null;
  pendingHandoverRecipientDisplayName: string | null;
  preHandoverOperationalStatus: AssetOperationalStatus | null;
  historySequence: number;
  created: string;
  createdBy: string | null;
  lastModified: string;
  lastModifiedBy: string | null;
  concurrencyToken: string;
}

export interface AssetDetailsRequest {
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  description: string | null;
  manufacturerSerialNumber: string | null;
  concurrencyToken: string;
  note: string | null;
}

export interface RegisteredAsset {
  id: string;
  numberId: number;
  assetCode: string;
  qrIdentifier: string;
  concurrencyToken: string;
}

export interface AssetEventSnapshot {
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  description: string | null;
  qrIdentifier: string;
  manufacturerSerialNumber: string | null;
  siteId: string;
  siteName: string | null;
  responsibleUserId: string | null;
  responsibleUserDisplayName: string | null;
  pendingHandoverRecipientId: string | null;
  pendingHandoverRecipientDisplayName: string | null;
  condition: AssetCondition;
  operationalStatus: AssetOperationalStatus;
  preHandoverOperationalStatus: AssetOperationalStatus | null;
  reviewStatus: AssetReviewStatus;
}

export interface AssetEvent {
  id: string;
  sequenceNumber: number;
  eventCode: string;
  actorId: string;
  actorDisplayName: string;
  occurredAt: string;
  note: string | null;
  before: AssetEventSnapshot | null;
  after: AssetEventSnapshot;
}

export interface SiteLookup {
  id: string;
  numberId: number;
  name: string;
  address: string;
  isCompanyStock: boolean;
}

export interface UserLookup {
  id: string;
  displayName: string;
  email: string | null;
}

export interface RegisterAssetsRequest {
  classification: AssetClassification;
  name: string;
  manufacturer: string | null;
  model: string | null;
  description: string | null;
  siteId: string;
  responsibleUserId: string | null;
  condition: AssetCondition;
  operationalStatus: AssetOperationalStatus;
  note: string | null;
  rows: readonly { manufacturerSerialNumber: string | null }[];
}

export interface AssetActionRequest {
  concurrencyToken: string;
  note?: string | null;
  [key: string]: unknown;
}

export interface ApiProblem {
  status?: number;
  title?: string;
  detail?: string;
  code?: string;
}
