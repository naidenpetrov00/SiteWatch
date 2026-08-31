export interface IssueRequest {
  siteId: string;
  title: string;
  description: string;
  status: string;
  startDate: string | null;
  endDate: string | null;
  assignedWorkerIds: readonly string[];
}

export interface UpdateIssueRequest extends IssueRequest {
  id: string;
}
