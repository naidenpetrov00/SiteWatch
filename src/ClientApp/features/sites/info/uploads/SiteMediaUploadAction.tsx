import { useCallback, useMemo, useState } from "react";
import { Pressable, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import {
  FILE_DOCUMENT_TYPE_LABELS,
  FILE_DOCUMENT_TYPES,
  type FileDocumentType,
} from "../files/types";
import type { MediaCategory } from "../media-types";
import { MAX_UPLOAD_BYTES } from "./constants";
import SiteUploadAction, { type UploadSourceOption } from "./SiteUploadAction";
import styles from "./SiteUploadAction.styles";
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
  const colorPalette = useColorPalette();
  const upload = useUploadSiteMedia();
  const [category, setCategory] = useState<MediaCategory | null>(null);
  const [documentType, setDocumentType] = useState<FileDocumentType | null>(null);
  const label = labelForKind(kind);
  const classificationSelected = kind === "file" ? documentType !== null : category !== null;
  const hasClassificationOptions = kind === "file" || allowedCategories.length > 0;

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
      return "Choose a JPEG, PNG, WebP, or GIF image.";
    }
    if (kind === "video" && !VIDEO_CONTENT_TYPES.has(asset.contentType)) {
      return "Choose an MP4, MOV, or WebM video.";
    }
    if (kind === "file" && !asset.contentType.trim()) {
      return "The selected file type is unavailable. Choose another file.";
    }
    return null;
  }, [kind, label]);

  const uploadAsset = useCallback((asset: UploadAsset) =>
    upload.mutateAsync({
      siteId: siteId!,
      kind,
      asset,
      category: category ?? undefined,
      documentType: documentType ?? undefined,
    }), [category, documentType, kind, siteId, upload]);

  const panelContent = (
    <>
      {hasClassificationOptions ? (
        <View style={styles.options}>
          {(kind === "file" ? FILE_DOCUMENT_TYPES : allowedCategories).map((option) => {
            const selected = kind === "file" ? documentType === option : category === option;
            const optionLabel = kind === "file"
              ? FILE_DOCUMENT_TYPE_LABELS[option as FileDocumentType]
              : option;

            return (
              <Pressable
                key={option}
                accessibilityRole="button"
                accessibilityState={{ selected }}
                disabled={upload.isPending}
                onPress={() => {
                  if (kind === "file") setDocumentType(option as FileDocumentType);
                  else setCategory(option as MediaCategory);
                }}
                style={({ pressed }) => [
                  styles.option,
                  {
                    backgroundColor: selected ? colorPalette.primary : colorPalette.background,
                    borderColor: selected ? colorPalette.primary : colorPalette.secondary,
                  },
                  pressed ? styles.pressed : null,
                ]}
              >
                <Text style={[styles.optionText, { color: selected ? colorPalette.contrastText : colorPalette.text }]}>
                  {optionLabel}
                </Text>
              </Pressable>
            );
          })}
        </View>
      ) : (
        <Text style={{ color: colorPalette.secondary }}>
          This site has no allowed {label} categories.
        </Text>
      )}
    </>
  );

  return (
    <SiteUploadAction
      canUpload={classificationSelected && hasClassificationOptions}
      documentPickerTypes={kind === "image" ? "image/*" : kind === "video" ? "video/*" : "*/*"}
      fallbackFileName={() => `${label}-${Date.now()}`}
      isUploading={upload.isPending}
      label={label}
      onUpload={uploadAsset}
      panelContent={panelContent}
      panelTitle={kind === "file" ? "Choose document type" : "Choose category"}
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
