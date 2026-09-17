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
