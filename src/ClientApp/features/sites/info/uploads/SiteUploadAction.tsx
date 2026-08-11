import { type ReactNode, useCallback, useState } from "react";
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
import { ACCESS_POLICIES, type UserRole } from "@/types/authorization";
import styles from "./SiteUploadAction.styles";
import type { UploadAsset } from "./types";

export type UploadSource = "camera" | "gallery" | "file";
export type UploadSourceOption = {
  source: UploadSource;
  label: string;
};

type PickerMediaKind = "image" | "video";

type SiteUploadActionProps = {
  label: string;
  siteId?: string;
  sourceOptions: readonly UploadSourceOption[];
  pickerMediaKind?: PickerMediaKind;
  documentPickerTypes: string | string[];
  allowedRoles?: readonly UserRole[];
  isUploading: boolean;
  canUpload?: boolean;
  panelTitle?: string;
  panelContent?: ReactNode;
  onUpload: (asset: UploadAsset) => Promise<unknown>;
  validateAsset: (asset: UploadAsset) => string | null;
  resolveContentType: (contentType: string | null | undefined, fileName: string) => string | null;
  fallbackFileName: () => string;
};

const getUploadErrorMessage = (error: unknown, fallback: string) =>
  error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;

const SiteUploadAction = ({
  label,
  siteId,
  sourceOptions,
  pickerMediaKind,
  documentPickerTypes,
  allowedRoles = ACCESS_POLICIES.siteMediaUpload,
  isUploading,
  canUpload = true,
  panelTitle,
  panelContent,
  onUpload,
  validateAsset,
  resolveContentType,
  fallbackFileName,
}: SiteUploadActionProps) => {
  const colorPalette = useColorPalette();
  const { bottom } = useSafeAreaInsets();
  const [isOpen, setIsOpen] = useState(false);
  const [activeSource, setActiveSource] = useState<UploadSource | null>(null);
  const isBusy = isUploading || activeSource !== null;

  const showUploadFailure = useCallback((message: string) => {
    Alert.alert("Upload failed", message);
  }, []);

  const showPermissionDenied = useCallback((source: "Camera" | "Photos") => {
    Alert.alert(
      `${source} permission required`,
      `Allow ${source.toLowerCase()} access to add a ${label}.`,
      [
        { text: "Cancel", style: "cancel" },
        { text: "Open Settings", onPress: () => void Linking.openSettings() },
      ],
    );
  }, [label]);

  const uploadAsset = useCallback((asset: UploadAsset) => {
    if (!siteId) {
      showUploadFailure("The site is unavailable. Return to the site and retry.");
      return;
    }

    const validationMessage = validateAsset(asset);
    if (validationMessage) {
      showUploadFailure(validationMessage);
      return;
    }

    setIsOpen(false);
    void onUpload(asset).catch((error) => {
      showUploadFailure(getUploadErrorMessage(error, `Unable to upload the ${label}.`));
    });
  }, [label, onUpload, showUploadFailure, siteId, validateAsset]);

  const toUploadAsset = useCallback((asset: {
    uri: string;
    fileName?: string | null;
    mimeType?: string | null;
    fileSize?: number | null;
  }): UploadAsset | null => {
    const fileName = asset.fileName ?? fallbackFileName();
    const contentType = resolveContentType(asset.mimeType, fileName);
    if (!contentType) {
      showUploadFailure(`The selected ${label} type is unavailable. Choose another ${label}.`);
      return null;
    }

    return {
      uri: asset.uri,
      fileName,
      contentType,
      fileSize: asset.fileSize ?? undefined,
    };
  }, [fallbackFileName, label, resolveContentType, showUploadFailure]);

  const handleCamera = useCallback(async () => {
    setActiveSource("camera");
    try {
      const permission = await ImagePicker.requestCameraPermissionsAsync();
      if (!permission.granted) {
        showPermissionDenied("Camera");
        return;
      }

      const result = await ImagePicker.launchCameraAsync({
        mediaTypes: pickerMediaKind === "video" ? ["videos"] : ["images"],
        allowsEditing: false,
        quality: pickerMediaKind === "image" ? 0.9 : undefined,
      });
      if (result.canceled) return;

      const asset = toUploadAsset(result.assets[0]);
      if (asset) uploadAsset(asset);
    } catch (error) {
      showUploadFailure(getUploadErrorMessage(error, `Unable to capture or upload the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [label, pickerMediaKind, showPermissionDenied, showUploadFailure, toUploadAsset, uploadAsset]);

  const handleGallery = useCallback(async () => {
    setActiveSource("gallery");
    try {
      const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (!permission.granted) {
        showPermissionDenied("Photos");
        return;
      }

      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: pickerMediaKind === "video" ? ["videos"] : ["images"],
        allowsEditing: false,
        quality: pickerMediaKind === "image" ? 1 : undefined,
      });
      if (result.canceled) return;

      const asset = toUploadAsset(result.assets[0]);
      if (asset) uploadAsset(asset);
    } catch (error) {
      showUploadFailure(getUploadErrorMessage(error, `Unable to select or upload the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [label, pickerMediaKind, showPermissionDenied, showUploadFailure, toUploadAsset, uploadAsset]);

  const handleFilePicker = useCallback(async () => {
    setActiveSource("file");
    try {
      const result = await DocumentPicker.getDocumentAsync({
        type: documentPickerTypes,
        multiple: false,
        copyToCacheDirectory: true,
      });
      if (result.canceled) return;

      const selected = result.assets[0];
      const asset = toUploadAsset({
        uri: selected.uri,
        fileName: selected.name,
        mimeType: selected.mimeType,
        fileSize: selected.size,
      });
      if (asset) uploadAsset(asset);
    } catch (error) {
      showUploadFailure(getUploadErrorMessage(error, `Unable to select or upload the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [documentPickerTypes, label, showUploadFailure, toUploadAsset, uploadAsset]);

  const handleSource = useCallback((source: UploadSource) => {
    if (source === "camera") return void handleCamera();
    if (source === "gallery") return void handleGallery();
    return void handleFilePicker();
  }, [handleCamera, handleFilePicker, handleGallery]);

  return (
    <RoleGate allowedRoles={allowedRoles}>
      <View
        style={[
          styles.container,
          { bottom: bottom + 12, left: 16, position: "absolute", right: 16 },
        ]}
      >
        {isOpen ? (
          <View style={[styles.panel, { backgroundColor: colorPalette.background, borderColor: `${colorPalette.secondary}88` }]}>
            {panelTitle ? <Text style={[styles.panelTitle, { color: colorPalette.text }]}>{panelTitle}</Text> : null}
            {panelContent}
            {sourceOptions.map(({ source, label: sourceLabel }) => (
              <Pressable
                key={source}
                accessibilityRole="button"
                disabled={!canUpload || isBusy}
                onPress={() => handleSource(source)}
                style={({ pressed }) => [
                  styles.sourceButton,
                  {
                    borderColor: `${colorPalette.secondary}88`,
                    opacity: !canUpload || isBusy ? 0.5 : 1,
                  },
                  pressed ? styles.pressed : null,
                ]}
              >
                {activeSource === source || isUploading ? <ActivityIndicator color={colorPalette.primary} /> : null}
                <Text style={[styles.sourceText, { color: colorPalette.text }]}>{sourceLabel}</Text>
              </Pressable>
            ))}
          </View>
        ) : null}
        <Pressable
          accessibilityRole="button"
          accessibilityState={{ expanded: isOpen }}
          disabled={isBusy}
          onPress={() => setIsOpen((open) => !open)}
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

export default SiteUploadAction;
