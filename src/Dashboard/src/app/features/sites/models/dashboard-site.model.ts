import { SiteMediaPolicy } from './site-media-policy-presets';

export interface DashboardSiteUser {
  id: string;
  displayName: string;
  email: string | null;
}

export interface DashboardSite {
  id: string;
  numberId: number;
  name: string;
  address: string;
  managerId: string;
  managerDisplayName: string;
  primaryClientUser?: DashboardSiteUser | null;
  startDate: string;
  endDate: string | null;
  status: string;
  mediaPolicy: SiteMediaPolicy;
  accessUsers?: readonly DashboardSiteUser[];
  /** Dashboard-only table action; not supplied by the API. */
  media?: string;
}
