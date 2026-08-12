import type { FileDocumentType } from "../files/types";
import type { MediaCategory } from "../media-types";

export type SiteMediaUploadKind = "image" | "video" | "file";

export type UploadAsset = {
  uri: string;
  fileName: string;
  contentType: string;
  fileSize?: number;
};

export type UploadSiteMediaRequest = {
  siteId: string;
  kind: SiteMediaUploadKind;
  asset: UploadAsset;
  category?: MediaCategory;
  documentType?: FileDocumentType;
};

export type PendingSiteMediaUpload = {
  mutationId: number;
  request: UploadSiteMediaRequest;
};
