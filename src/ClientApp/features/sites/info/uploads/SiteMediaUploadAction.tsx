import { useCallback, useMemo } from "react";
import {
  FILE_DOCUMENT_TYPE_LABELS,
  FILE_DOCUMENT_TYPES,
  type FileDocumentType,
} from "../files/types";
import type { MediaCategory } from "../media-types";
import { MAX_UPLOAD_BYTES } from "./constants";
import SiteUploadAction, { type UploadSourceOption } from "./SiteUploadAction";
import type { SiteMediaUploadKind, UploadAsset } from "./types";
import { useUploadSiteMedia } from "./useUploadSiteMedia";

type SiteMediaUploadActionProps = {
  kind: SiteMediaUploadKind;
  siteId?: string;
  allowedCategories?: readonly MediaCategory[];
};

const IMAGE_CONTENT_TYPES = new Set([
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/gif",
  "image/heic",
  "image/heif",
]);
const VIDEO_CONTENT_TYPES = new Set([
  "video/mp4",
  "video/quicktime",
  "video/webm",
]);

const contentTypeFromFileName = (fileName: string): string | null => {
  const extension = fileName.split(".").pop()?.toLowerCase();
  const contentTypes: Record<string, string> = {
    gif: "image/gif",
    heic: "image/heic",
    heif: "image/heif",
    jpeg: "image/jpeg",
    jpg: "image/jpeg",
    mov: "video/quicktime",
    mp4: "video/mp4",
    png: "image/png",
    webm: "video/webm",
    webp: "image/webp",
  };

  return extension ? contentTypes[extension] ?? null : null;
};

const normaliseContentType = (contentType: string | null | undefined) =>
  contentType === "image/jpg" ? "image/jpeg" : contentType?.toLowerCase();

const labelForKind = (kind: SiteMediaUploadKind) =>
  kind === "image" ? "image" : kind === "video" ? "video" : "file";

const SiteMediaUploadAction = ({
  kind,
  siteId,
  allowedCategories = [],
}: SiteMediaUploadActionProps) => {
  const upload = useUploadSiteMedia();
  const label = labelForKind(kind);

  const sourceOptions = useMemo<readonly UploadSourceOption[]>(
    () =>
      kind === "file"
        ? [{ source: "file", label: "Choose a file" }]
        : [
            { source: "camera", label: kind === "image" ? "Take a photo" : "Record a video" },
            { source: "gallery", label: "Choose from photo library" },
            { source: "file", label: "Choose from phone files" },
          ],
    [kind],
  );

  const validateAsset = useCallback((asset: UploadAsset): string | null => {
    const maxSize = MAX_UPLOAD_BYTES[kind];
    if (asset.fileSize === 0) return `Choose a non-empty ${label} file.`;
    if (asset.fileSize !== undefined && asset.fileSize > maxSize) {
      return `The ${label} file cannot exceed ${Math.round(maxSize / 1024 / 1024)} MB.`;
    }
    if (kind === "image" && !IMAGE_CONTENT_TYPES.has(asset.contentType)) {
      return "Choose a JPEG, PNG, WebP, GIF, HEIC, or HEIF image.";
    }
    if (kind === "video" && !VIDEO_CONTENT_TYPES.has(asset.contentType)) {
      return "Choose an MP4, MOV, or WebM video.";
    }
    if (kind === "file" && !asset.contentType.trim()) {
      return "The selected file type is unavailable. Choose another file.";
    }
    return null;
  }, [kind, label]);

  const uploadAsset = useCallback((asset: UploadAsset, classification?: string) =>
    upload.mutateAsync({
      siteId: siteId!,
      kind,
      asset,
      category: kind === "file" ? undefined : classification as MediaCategory | undefined,
      documentType: kind === "file" ? classification as FileDocumentType | undefined : undefined,
    }), [kind, siteId, upload]);

  const classification = useMemo(() => ({
    title: kind === "file" ? "Choose document type" : "Choose category",
    options: (kind === "file" ? FILE_DOCUMENT_TYPES : allowedCategories).map((option) => ({
      value: option,
      label: kind === "file"
        ? FILE_DOCUMENT_TYPE_LABELS[option as FileDocumentType]
        : option,
    })),
  }), [allowedCategories, kind]);

  return (
    <SiteUploadAction
      classification={classification}
      documentPickerTypes={kind === "image" ? "image/*" : kind === "video" ? "video/*" : "*/*"}
      fallbackFileName={() => `${label}-${Date.now()}`}
      isUploading={upload.isPending}
      label={label}
      onUpload={uploadAsset}
      pickerMediaKind={kind === "video" ? "video" : "image"}
      resolveContentType={(contentType, fileName) =>
        normaliseContentType(contentType) ?? contentTypeFromFileName(fileName) ?? (kind === "file" ? "application/octet-stream" : null)}
      siteId={siteId}
      sourceOptions={sourceOptions}
      validateAsset={validateAsset}
    />
  );
};

export default SiteMediaUploadAction;
