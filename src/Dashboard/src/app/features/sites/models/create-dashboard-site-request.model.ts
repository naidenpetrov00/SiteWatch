import { SiteMediaPolicyPreset } from './site-media-policy-presets';

export interface CreateDashboardSiteRequest {
  name: string;
  address: string;
  managerId: string;
  primaryClientUserId?: string | null;
  userIds?: readonly string[];
  startDate: string;
  endDate: string | null;
  status: string;
  mediaPolicyPreset: SiteMediaPolicyPreset;
  mediaCategories: readonly string[];
}
