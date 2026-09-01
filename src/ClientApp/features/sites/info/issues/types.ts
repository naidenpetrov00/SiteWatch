export type IssueWorker = {
  id: string;
  userName: string | null;
  email: string | null;
};

export type SiteIssue = {
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
  assignedWorkers: IssueWorker[];
};

export type CreateIssueRequest = {
  siteId: string;
  title: string;
  description: string;
};

export type CreateIssueResponse = {
  id: string;
};
