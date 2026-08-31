export interface DashboardIssueWorker {
  id: string;
  userName: string | null;
  email: string | null;
}

export interface DashboardIssue {
  id: string;
  numberId: number;
  siteId: string;
  siteName: string;
  title: string;
  description: string;
  status: string;
  startDate: string | null;
  endDate: string | null;
  created: string;
  createdBy: string | null;
  lastModified: string;
  lastModifiedBy: string | null;
  assignedWorkers: readonly DashboardIssueWorker[];
  worker?: string;
}
