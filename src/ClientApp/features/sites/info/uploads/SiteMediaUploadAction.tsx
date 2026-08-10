import { useCallback, useMemo, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Linking,
  Pressable,
  Text,
  View,
} from "react-native";
import * as DocumentPicker from "expo-document-picker";
import * as ImagePicker from "expo-image-picker";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import RoleGate from "@/features/auth/components/RoleGate/RoleGate";
import { useColorPalette } from "@/hooks/useColorPalette";
import { ACCESS_POLICIES } from "@/types/authorization";
import {
  FILE_DOCUMENT_TYPE_LABELS,
  FILE_DOCUMENT_TYPES,
  type FileDocumentType,
} from "../files/types";
import type { MediaCategory } from "../media-types";
import { MAX_UPLOAD_BYTES, UPLOAD_ERROR_COLORS } from "./constants";
import styles from "./SiteMediaUploadAction.styles";
import type { SiteMediaUploadKind, UploadAsset } from "./types";
import { useUploadSiteMedia } from "./useUploadSiteMedia";

type UploadSource = "camera" | "gallery" | "file";

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

const getUploadErrorMessage = (error: unknown, fallback: string) =>
  error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;

const SiteMediaUploadAction = ({
  kind,
  siteId,
  allowedCategories = [],
}: SiteMediaUploadActionProps) => {
  const colorPalette = useColorPalette();
  const { bottom } = useSafeAreaInsets();
  const upload = useUploadSiteMedia();
  const [isOpen, setIsOpen] = useState(false);
  const [category, setCategory] = useState<MediaCategory | null>(null);
  const [documentType, setDocumentType] = useState<FileDocumentType | null>(null);
  const [activeSource, setActiveSource] = useState<UploadSource | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const label = labelForKind(kind);
  const classificationSelected = kind === "file" ? documentType !== null : category !== null;
  const hasClassificationOptions = kind === "file" || allowedCategories.length > 0;
  const isBusy = upload.isPending || activeSource !== null;
  const sourceOptions = useMemo(
    () =>
      kind === "file"
        ? ([["file", "Choose a file"]] as const)
        : ([
            ["camera", kind === "image" ? "Take a photo" : "Record a video"],
            ["gallery", "Choose from photo library"],
            ["file", "Choose from phone files"],
          ] as const),
    [kind],
  );

  const clearFeedback = useCallback(() => {
    setError(null);
    setMessage(null);
  }, []);

  const showPermissionDenied = useCallback((source: "Camera" | "Photos") => {
    Alert.alert(
      `${source} permission required`,
      `Allow ${source.toLowerCase()} access to add a site ${label}.`,
      [
        { text: "Cancel", style: "cancel" },
        { text: "Open Settings", onPress: () => void Linking.openSettings() },
      ],
    );
  }, [label]);

  const validateAndUpload = useCallback(async (asset: UploadAsset) => {
    const maxSize = MAX_UPLOAD_BYTES[kind];
    if (asset.fileSize === 0) {
      setError(`Choose a non-empty ${label} file.`);
      return;
    }

    if (asset.fileSize !== undefined && asset.fileSize > maxSize) {
      setError(`The ${label} file cannot exceed ${Math.round(maxSize / 1024 / 1024)} MB.`);
      return;
    }

    if (kind === "image" && !IMAGE_CONTENT_TYPES.has(asset.contentType)) {
      setError("Choose a JPEG, PNG, WebP, or GIF image.");
      return;
    }

    if (kind === "video" && !VIDEO_CONTENT_TYPES.has(asset.contentType)) {
      setError("Choose an MP4, MOV, or WebM video.");
      return;
    }

    if (kind === "file" && !asset.contentType.trim()) {
      setError("The selected file type is unavailable. Choose another file.");
      return;
    }

    if (!siteId) {
      setError("The site is unavailable. Return to the site and retry.");
      return;
    }

    if (kind === "file" && !documentType) {
      setError("Choose a document type before uploading.");
      return;
    }

    if (kind !== "file" && !category) {
      setError("Choose a category before uploading.");
      return;
    }

    setError(null);
    setMessage(null);
    await upload.mutateAsync({
      siteId,
      kind,
      asset,
      category: category ?? undefined,
      documentType: documentType ?? undefined,
    });
    setMessage(`${label[0].toUpperCase()}${label.slice(1)} uploaded.`);
    setIsOpen(false);
  }, [category, documentType, kind, label, siteId, upload]);

  const toUploadAsset = useCallback((asset: {
    uri: string;
    fileName?: string | null;
    mimeType?: string | null;
    fileSize?: number | null;
  }): UploadAsset | null => {
    const fileName = asset.fileName ?? `${label}-${Date.now()}`;
    const contentType = normaliseContentType(asset.mimeType) ?? contentTypeFromFileName(fileName);

    if (!contentType && kind !== "file") {
      setError(`The selected ${label} type is unavailable. Choose another ${label}.`);
      return null;
    }

    return {
      uri: asset.uri,
      fileName,
      contentType: contentType ?? "application/octet-stream",
      fileSize: asset.fileSize ?? undefined,
    };
  }, [kind, label]);

  const handleCamera = useCallback(async () => {
    setActiveSource("camera");
    clearFeedback();
    try {
      const permission = await ImagePicker.requestCameraPermissionsAsync();
      if (!permission.granted) {
        showPermissionDenied("Camera");
        return;
      }

      const result = await ImagePicker.launchCameraAsync({
        mediaTypes: kind === "image" ? ["images"] : ["videos"],
        allowsEditing: false,
        quality: kind === "image" ? 0.9 : undefined,
      });
      if (result.canceled) return;

      const asset = toUploadAsset(result.assets[0]);
      if (asset) await validateAndUpload(asset);
    } catch (caughtError) {
      setError(getUploadErrorMessage(caughtError, `Unable to capture or upload the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [clearFeedback, kind, label, showPermissionDenied, toUploadAsset, validateAndUpload]);

  const handleGallery = useCallback(async () => {
    setActiveSource("gallery");
    clearFeedback();
    try {
      const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (!permission.granted) {
        showPermissionDenied("Photos");
        return;
      }

      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: kind === "image" ? ["images"] : ["videos"],
        allowsEditing: false,
        quality: kind === "image" ? 1 : undefined,
      });
      if (result.canceled) return;

      const asset = toUploadAsset(result.assets[0]);
      if (asset) await validateAndUpload(asset);
    } catch (caughtError) {
      setError(getUploadErrorMessage(caughtError, `Unable to select or upload the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [clearFeedback, kind, label, showPermissionDenied, toUploadAsset, validateAndUpload]);

  const handleFilePicker = useCallback(async () => {
    setActiveSource("file");
    clearFeedback();
    try {
      const result = await DocumentPicker.getDocumentAsync({
        type: kind === "image" ? "image/*" : kind === "video" ? "video/*" : "*/*",
        multiple: false,
        copyToCacheDirectory: true,
      });
      if (result.canceled) return;

      const asset = toUploadAsset({
        uri: result.assets[0].uri,
        fileName: result.assets[0].name,
        mimeType: result.assets[0].mimeType,
        fileSize: result.assets[0].size,
      });
      if (asset) await validateAndUpload(asset);
    } catch (caughtError) {
      setError(getUploadErrorMessage(caughtError, `Unable to select or upload the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [clearFeedback, kind, label, toUploadAsset, validateAndUpload]);

  const handleSource = useCallback((source: UploadSource) => {
    if (source === "camera") return void handleCamera();
    if (source === "gallery") return void handleGallery();
    return void handleFilePicker();
  }, [handleCamera, handleFilePicker, handleGallery]);

  const toggleOpen = useCallback(() => {
    setIsOpen((open) => !open);
    clearFeedback();
  }, [clearFeedback]);

  return (
    <RoleGate allowedRoles={ACCESS_POLICIES.siteMediaUpload}>
      <View
        style={[
          styles.container,
          { bottom: bottom + 12, left: 16, position: "absolute", right: 16 },
        ]}
      >
        {message ? (
          <Text accessibilityRole="alert" style={[styles.feedback, { backgroundColor: "rgba(22, 163, 74, 0.12)", borderColor: UPLOAD_ERROR_COLORS.success, color: colorPalette.text }]}>
            {message}
          </Text>
        ) : null}
        {error ? (
          <Text accessibilityRole="alert" style={[styles.feedback, { borderColor: UPLOAD_ERROR_COLORS.error, color: UPLOAD_ERROR_COLORS.error }]}>
            {error}
          </Text>
        ) : null}
        {isOpen ? (
          <View style={[styles.panel, { backgroundColor: colorPalette.background, borderColor: `${colorPalette.secondary}88` }]}>
            <Text style={[styles.panelTitle, { color: colorPalette.text }]}>
              {kind === "file" ? "Choose document type" : "Choose category"}
            </Text>
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
                      disabled={isBusy}
                      onPress={() => {
                        if (kind === "file") setDocumentType(option as FileDocumentType);
                        else setCategory(option as MediaCategory);
                        setError(null);
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

            {sourceOptions.map(([source, sourceLabel]) => (
              <Pressable
                key={source}
                accessibilityRole="button"
                disabled={!classificationSelected || !hasClassificationOptions || isBusy}
                onPress={() => handleSource(source)}
                style={({ pressed }) => [
                  styles.sourceButton,
                  {
                    borderColor: `${colorPalette.secondary}88`,
                    opacity: !classificationSelected || !hasClassificationOptions || isBusy ? 0.5 : 1,
                  },
                  pressed ? styles.pressed : null,
                ]}
              >
                {activeSource === source || upload.isPending ? <ActivityIndicator color={colorPalette.primary} /> : null}
                <Text style={[styles.sourceText, { color: colorPalette.text }]}>{sourceLabel}</Text>
              </Pressable>
            ))}
          </View>
        ) : null}
        <Pressable
          accessibilityRole="button"
          accessibilityState={{ expanded: isOpen }}
          disabled={isBusy}
          onPress={toggleOpen}
          style={({ pressed }) => [
            styles.actionButton,
            { backgroundColor: colorPalette.primary, opacity: isBusy ? 0.6 : 1 },
            pressed ? styles.pressed : null,
          ]}
        >
          <Text style={[styles.actionButtonText, { color: colorPalette.contrastText }]}>
            {isOpen ? "Close upload" : `Upload ${label}`}
          </Text>
        </Pressable>
      </View>
    </RoleGate>
  );
};

export default SiteMediaUploadAction;
