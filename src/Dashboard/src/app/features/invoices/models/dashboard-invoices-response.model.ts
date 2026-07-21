import { DashboardInvoice } from './dashboard-invoice.model';

export interface DashboardInvoicesResponse {
  items: readonly DashboardInvoice[];
  filteredCount: number;
  totalCount: number;
}
