import { DashboardSite } from './dashboard-site.model';

export interface DashboardSitesResponse {
  items: readonly DashboardSite[];
  filteredCount: number;
  totalCount: number;
}
