import {
  IssueAttachment,
  IssueAttachmentKind
} from '../../models/issue-attachment.model';

export type AttachmentOperationStatus =
  | 'idle'
  | 'queued'
  | 'uploading'
  | 'deleting'
  | 'error';

export interface ManagedIssueAttachment {
  attachment: IssueAttachment;
  markedForRemoval: boolean;
  status: AttachmentOperationStatus;
  error: string | null;
  previewUrl: string | null;
}

export interface QueuedIssueAttachment {
  clientId: string;
  file: File;
  kind: IssueAttachmentKind;
  contentType: string;
  previewUrl: string | null;
  status: AttachmentOperationStatus;
  progress: number | null;
  error: string | null;
}

export interface IssueAttachmentViewer {
  kind: 'Image' | 'Video';
  name: string;
  url: string;
}
