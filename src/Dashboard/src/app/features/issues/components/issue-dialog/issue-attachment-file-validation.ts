import { IssueAttachmentKind } from '../../models/issue-attachment.model';

const IMAGE_MAX_FILE_SIZE = 50 * 1024 * 1024;
const VIDEO_MAX_FILE_SIZE = 500 * 1024 * 1024;
const FILE_MAX_FILE_SIZE = 100 * 1024 * 1024;
const MAX_FILE_NAME_LENGTH = 512;
const MAX_CONTENT_TYPE_LENGTH = 128;

const IMAGE_CONTENT_TYPES = new Set([
  'image/jpeg',
  'image/png',
  'image/webp',
  'image/gif',
  'image/heic',
  'image/heif'
]);

const VIDEO_CONTENT_TYPES = new Set([
  'video/mp4',
  'video/quicktime',
  'video/webm'
]);

export interface ValidIssueAttachmentFile {
  kind: IssueAttachmentKind;
  contentType: string;
}

export function validateIssueAttachmentFile(
  file: File
): ValidIssueAttachmentFile | string {
  const contentType = normalizeContentType(file.type);

  if (file.name.trim().length === 0 || file.name.length > MAX_FILE_NAME_LENGTH) {
    return `The file name is required and must be at most ${MAX_FILE_NAME_LENGTH} characters.`;
  }
  if (contentType.length > MAX_CONTENT_TYPE_LENGTH) {
    return `The file content type must be at most ${MAX_CONTENT_TYPE_LENGTH} characters.`;
  }

  const classification = classifyContentType(contentType);
  if (typeof classification === 'string') {
    return classification;
  }

  if (file.size <= 0) {
    return 'The file cannot be empty.';
  }
  if (file.size > classification.maxSize) {
    return `The ${classification.kind.toLowerCase()} cannot exceed ${classification.maxSize / 1024 / 1024} MB.`;
  }

  return { kind: classification.kind, contentType };
}

function classifyContentType(contentType: string):
  | { kind: IssueAttachmentKind; maxSize: number }
  | string {
  if (IMAGE_CONTENT_TYPES.has(contentType)) {
    return { kind: 'Image', maxSize: IMAGE_MAX_FILE_SIZE };
  }
  if (VIDEO_CONTENT_TYPES.has(contentType)) {
    return { kind: 'Video', maxSize: VIDEO_MAX_FILE_SIZE };
  }
  if (contentType.startsWith('image/')) {
    return 'Only JPEG, PNG, WebP, GIF, HEIC, or HEIF images are supported.';
  }
  if (contentType.startsWith('video/')) {
    return 'Only MP4, MOV, or WebM videos are supported.';
  }

  return { kind: 'File', maxSize: FILE_MAX_FILE_SIZE };
}

function normalizeContentType(contentType: string): string {
  const normalized = contentType.trim().toLowerCase();
  if (!normalized) return 'application/octet-stream';
  return normalized === 'image/jpg' ? 'image/jpeg' : normalized;
}
