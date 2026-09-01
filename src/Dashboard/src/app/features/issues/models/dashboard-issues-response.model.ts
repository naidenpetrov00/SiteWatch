import { DashboardIssue } from './dashboard-issue.model';

export interface DashboardIssuesResponse {
  items: readonly DashboardIssue[];
  filteredCount: number;
  totalCount: number;
}
