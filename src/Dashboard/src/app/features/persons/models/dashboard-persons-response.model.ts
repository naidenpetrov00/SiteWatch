import { DashboardPerson } from './dashboard-person.model';

export interface DashboardPersonsResponse {
  items: readonly DashboardPerson[];
  filteredCount: number;
  totalCount: number;
}
