import { SiteMediaPolicyPreset } from './site-media-policy-presets';

export interface UpdateDashboardSiteRequest {
  id: string;
  name: string;
  address: string;
  managerId: string;
  startDate: string;
  endDate: string | null;
  status: string;
  mediaPolicyPreset: SiteMediaPolicyPreset;
  mediaCategoriesToAdd: readonly string[];
}
