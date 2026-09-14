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

export type IssueAttachmentKind = "Image" | "Video" | "File";

export type IssueAttachment = {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  kind: IssueAttachmentKind;
  hasPreview: boolean;
  durationSeconds: number | null;
  created: string;
};

export type IssueAttachmentAccess = {
  url: string;
  expiresAt: string;
};

export type PendingIssueAttachment = {
  clientId: string;
  uri: string;
  fileName: string;
  contentType: string;
  fileSize?: number;
  kind: IssueAttachmentKind;
  status: "queued" | "uploading" | "error";
  error: string | null;
};
