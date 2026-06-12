import { DashboardUser } from './dashboard-user.model';

export interface DashboardUsersResponse {
  items: readonly DashboardUser[];
  filteredCount: number;
  totalCount: number;
}
