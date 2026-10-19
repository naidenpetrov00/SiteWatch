export type AssetClassification = 'Machine' | 'Tool';
export type AssetReviewStatus = 'PendingReview' | 'Active' | 'Rejected';
export type AssetCondition = 'Good' | 'Damaged' | 'Unsafe';
export type AssetOperationalStatus = 'Available' | 'InUse' | 'InTransfer' | 'Missing' | 'UnderRepair' | 'Retired';

export const ASSET_CLASSIFICATIONS: readonly AssetClassification[] = ['Machine', 'Tool'];
export const ASSET_CONDITIONS: readonly AssetCondition[] = ['Good', 'Damaged', 'Unsafe'];
export const REGISTRATION_STATUSES: readonly AssetOperationalStatus[] = ['Available', 'InUse', 'UnderRepair'];
export const REVIEW_STATUSES: readonly AssetReviewStatus[] = ['PendingReview', 'Active', 'Rejected'];

export interface PagedResponse<T> { items: readonly T[]; filteredCount: number; totalCount: number; }
export interface CatalogListItem { id: string; classification: AssetClassification; name: string; manufacturer: string | null; model: string | null; reviewStatus: AssetReviewStatus; created: string; lastModified: string; }
export interface CatalogLookup { id: string; classification: AssetClassification; name: string; manufacturer: string | null; model: string | null; }
export interface CatalogDetails extends CatalogListItem { description: string | null; reviewedById: string | null; reviewedByDisplayName: string | null; reviewedAt: string | null; rejectionReason: string | null; createdBy: string | null; lastModifiedBy: string | null; concurrencyToken: string; }
export interface CatalogDetailsRequest { classification: AssetClassification; name: string; manufacturer: string | null; model: string | null; description: string | null; }

export interface AssetListItem { id: string; numberId: number; assetCode: string; qrIdentifier: string; manufacturerSerialNumber: string | null; catalogItemId: string; catalogName: string; classification: AssetClassification; siteId: string; siteName: string; responsibleWorkerId: string | null; responsibleWorkerDisplayName: string | null; condition: AssetCondition; operationalStatus: AssetOperationalStatus; reviewStatus: AssetReviewStatus; hasPendingHandover: boolean; }
export interface AssetDetails extends AssetListItem { manufacturer: string | null; model: string | null; pendingHandoverRecipientId: string | null; pendingHandoverRecipientDisplayName: string | null; preHandoverOperationalStatus: AssetOperationalStatus | null; historySequence: number; created: string; createdBy: string | null; lastModified: string; lastModifiedBy: string | null; concurrencyToken: string; }
export interface RegisteredAsset { id: string; numberId: number; assetCode: string; qrIdentifier: string; concurrencyToken: string; }
export interface AssetEventSnapshot { catalogItemId: string; catalogItemName: string | null; qrIdentifier: string; manufacturerSerialNumber: string | null; siteId: string; siteName: string | null; responsibleWorkerId: string | null; responsibleWorkerDisplayName: string | null; pendingHandoverRecipientId: string | null; pendingHandoverRecipientDisplayName: string | null; condition: AssetCondition; operationalStatus: AssetOperationalStatus; preHandoverOperationalStatus: AssetOperationalStatus | null; reviewStatus: AssetReviewStatus; }
export interface AssetEvent { id: string; sequenceNumber: number; eventCode: string; actorId: string; actorDisplayName: string; occurredAt: string; note: string | null; before: AssetEventSnapshot | null; after: AssetEventSnapshot; }
export interface SiteLookup { id: string; numberId: number; name: string; address: string; }
export interface UserLookup { id: string; displayName: string; email: string | null; }

export interface RegisterAssetsRequest { catalogItemId: string; siteId: string; responsibleWorkerId: string | null; condition: AssetCondition; operationalStatus: AssetOperationalStatus; note: string | null; rows: readonly { manufacturerSerialNumber: string | null }[]; }
export interface AssetActionRequest { concurrencyToken: string; note?: string | null; [key: string]: unknown; }
export interface ApiProblem { status?: number; title?: string; detail?: string; code?: string; }
