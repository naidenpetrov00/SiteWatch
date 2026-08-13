import { SiteMediaPolicy } from './site-media-policy-presets';

export interface DashboardSite {
  id: string;
  numberId: number;
  name: string;
  address: string;
  managerId: string;
  managerDisplayName: string;
  startDate: string;
  endDate: string | null;
  status: string;
  mediaPolicy: SiteMediaPolicy;
}
