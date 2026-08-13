import { SiteMediaPolicy } from './site-media-policy-presets';

export interface DashboardSite {
  id: string;
  numberId: number;
  name: string;
  address: string;
  mediaPolicy: SiteMediaPolicy;
}
