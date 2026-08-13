import { SiteMediaPolicyPreset } from './site-media-policy-presets';

export interface CreateDashboardSiteRequest {
  name: string;
  address: string;
  mediaPolicyPreset: SiteMediaPolicyPreset;
  mediaCategories: readonly string[];
}
