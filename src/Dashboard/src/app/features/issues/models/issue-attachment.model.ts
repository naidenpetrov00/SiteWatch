export type IssueAttachmentKind = 'Image' | 'Video' | 'File';

export interface IssueAttachment {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  kind: IssueAttachmentKind;
  hasPreview: boolean;
  durationSeconds: number | null;
  created: string;
}

export interface IssueAttachmentAccess {
  url: string;
  expiresAt: string;
}
