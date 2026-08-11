import { type ReactNode, useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  AccessibilityInfo,
  ActionSheetIOS,
  ActivityIndicator,
  Alert,
  Linking,
  Modal,
  Platform,
  Pressable,
  Text,
  View,
} from "react-native";
import * as DocumentPicker from "expo-document-picker";
import * as Haptics from "expo-haptics";
import * as ImagePicker from "expo-image-picker";
import { BlurView } from "expo-blur";
import { GlassView, isLiquidGlassAvailable } from "expo-glass-effect";
import Animated, { FadeInDown, FadeOutDown } from "react-native-reanimated";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import RoleGate from "@/features/auth/components/RoleGate/RoleGate";
import { useColorPalette } from "@/hooks/useColorPalette";
import { ACCESS_POLICIES, type UserRole } from "@/types/authorization";
import styles from "./SiteUploadAction.styles";
import type { UploadAsset } from "./types";

export type UploadSource = "camera" | "gallery" | "file";
export type UploadSourceOption = { source: UploadSource; label: string };
export type UploadClassificationOption = { value: string; label: string };

type PickerMediaKind = "image" | "video";

type SiteUploadActionProps = {
  label: string;
  siteId?: string;
  sourceOptions: readonly UploadSourceOption[];
  pickerMediaKind?: PickerMediaKind;
  documentPickerTypes: string | string[];
  classification?: { title: string; options: readonly UploadClassificationOption[] };
  allowedRoles?: readonly UserRole[];
  isUploading: boolean;
  onUpload: (asset: UploadAsset, classification?: string) => Promise<unknown>;
  validateAsset: (asset: UploadAsset) => string | null;
  resolveContentType: (contentType: string | null | undefined, fileName: string) => string | null;
  fallbackFileName: () => string;
};

const getUploadErrorMessage = (error: unknown, fallback: string) =>
  error instanceof Error && error.message.trim().length > 0 ? error.message : fallback;

const isIos = Platform.OS === "ios";

const SiteUploadAction = ({
  label,
  siteId,
  sourceOptions,
  pickerMediaKind,
  documentPickerTypes,
  classification,
  allowedRoles = ACCESS_POLICIES.siteMediaUpload,
  isUploading,
  onUpload,
  validateAsset,
  resolveContentType,
  fallbackFileName,
}: SiteUploadActionProps) => {
  const colorPalette = useColorPalette();
  const { bottom } = useSafeAreaInsets();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [activeSource, setActiveSource] = useState<UploadSource | null>(null);
  const [selectedClassification, setSelectedClassification] = useState<string | null>(null);
  const [retryAsset, setRetryAsset] = useState<UploadAsset | null>(null);
  const [reduceTransparency, setReduceTransparency] = useState(false);
  const selectedClassificationRef = useRef<string | null>(null);
  const retryUploadRef = useRef<(asset: UploadAsset) => void>(() => undefined);
  const isBusy = isUploading || activeSource !== null;
  const hasClassification = Boolean(classification);
  const hasOptions = !classification || classification.options.length > 0;
  const canChooseSource = !hasClassification || selectedClassification !== null;
  const useLiquidGlass = isIos && !reduceTransparency && isLiquidGlassAvailable();

  useEffect(() => {
    void AccessibilityInfo.isReduceTransparencyEnabled().then(setReduceTransparency);
  }, []);

  useEffect(() => {
    if (!classification) {
      selectedClassificationRef.current = null;
      setSelectedClassification(null);
      return;
    }

    const defaultClassification = classification.options.length === 1
      ? classification.options[0].value
      : null;
    selectedClassificationRef.current = defaultClassification;
    setSelectedClassification(defaultClassification);
  }, [classification]);

  const selectClassification = useCallback((value: string) => {
    selectedClassificationRef.current = value;
    setSelectedClassification(value);
  }, []);

  const haptic = useCallback(() => {
    if (isIos) void Haptics.selectionAsync();
  }, []);

  const showUploadFailure = useCallback((message: string, asset?: UploadAsset) => {
    setRetryAsset(asset ?? null);
    Alert.alert("Upload failed", message, asset ? [
      { text: "Cancel", style: "cancel" },
      { text: "Retry", onPress: () => retryUploadRef.current(asset) },
    ] : undefined);
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

    return { uri: asset.uri, fileName, contentType, fileSize: asset.fileSize ?? undefined };
  }, [fallbackFileName, label, resolveContentType, showUploadFailure]);

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

    setIsMenuOpen(false);
    setRetryAsset(null);
    const classificationValue = selectedClassificationRef.current
      ?? (classification?.options.length === 1 ? classification.options[0].value : undefined);
    void onUpload(asset, classificationValue).catch((error) => {
      showUploadFailure(getUploadErrorMessage(error, `Unable to upload the ${label}.`), asset);
    });
  }, [classification, label, onUpload, showUploadFailure, siteId, validateAsset]);

  retryUploadRef.current = uploadAsset;

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
      if (!result.canceled) {
        const asset = toUploadAsset(result.assets[0]);
        if (asset) uploadAsset(asset);
      }
    } catch (error) {
      showUploadFailure(getUploadErrorMessage(error, `Unable to capture the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [label, pickerMediaKind, showPermissionDenied, showUploadFailure, toUploadAsset, uploadAsset]);

  const handleGallery = useCallback(async () => {
    setActiveSource("gallery");
    try {
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: pickerMediaKind === "video" ? ["videos"] : ["images"],
        allowsEditing: false,
        quality: pickerMediaKind === "image" ? 1 : undefined,
      });
      if (!result.canceled) {
        const asset = toUploadAsset(result.assets[0]);
        if (asset) uploadAsset(asset);
      }
    } catch (error) {
      showUploadFailure(getUploadErrorMessage(error, `Unable to select the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [label, pickerMediaKind, showUploadFailure, toUploadAsset, uploadAsset]);

  const handleFilePicker = useCallback(async () => {
    setActiveSource("file");
    try {
      const result = await DocumentPicker.getDocumentAsync({
        type: documentPickerTypes,
        multiple: false,
        copyToCacheDirectory: true,
      });
      if (!result.canceled) {
        const selected = result.assets[0];
        const asset = toUploadAsset({
          uri: selected.uri,
          fileName: selected.name,
          mimeType: selected.mimeType,
          fileSize: selected.size,
        });
        if (asset) uploadAsset(asset);
      }
    } catch (error) {
      showUploadFailure(getUploadErrorMessage(error, `Unable to select the ${label}.`));
    } finally {
      setActiveSource(null);
    }
  }, [documentPickerTypes, label, showUploadFailure, toUploadAsset, uploadAsset]);

  const handleSource = useCallback((source: UploadSource) => {
    if (isBusy) return;
    haptic();
    if (source === "camera") return void handleCamera();
    if (source === "gallery") return void handleGallery();
    return void handleFilePicker();
  }, [canChooseSource, handleCamera, handleFilePicker, handleGallery, haptic, isBusy]);

  const showIosSources = useCallback(() => {
    ActionSheetIOS.showActionSheetWithOptions({
      options: [...sourceOptions.map((option) => option.label), "Cancel"],
      cancelButtonIndex: sourceOptions.length,
      title: `Add ${label}`,
    }, (index) => {
      const option = sourceOptions[index];
      if (option) handleSource(option.source);
    });
  }, [handleSource, label, sourceOptions]);

  const openIosFlow = useCallback(() => {
    if (!hasOptions) {
      Alert.alert(`No ${label} options`, `This site has no available ${classification?.title.toLowerCase() ?? "upload"} options.`);
      return;
    }

    haptic();
    if (!classification || classification.options.length === 1) {
      showIosSources();
      return;
    }

    ActionSheetIOS.showActionSheetWithOptions({
      options: [...classification.options.map((option) => option.label), "Cancel"],
      cancelButtonIndex: classification.options.length,
      title: classification.title,
    }, (index) => {
      const option = classification.options[index];
      if (!option) return;
      selectClassification(option.value);
      haptic();
      setTimeout(showIosSources, 0);
    });
  }, [classification, hasOptions, haptic, label, selectClassification, showIosSources]);

  const handleActionPress = useCallback(() => {
    if (isBusy) return;
    if (isIos) {
      openIosFlow();
      return;
    }
    setIsMenuOpen(true);
    haptic();
  }, [haptic, isBusy, openIosFlow]);

  const actionLabel = `Add ${label}`;
  const actionContent = useMemo(() => (
    <Pressable
      accessibilityLabel={actionLabel}
      accessibilityRole="button"
      accessibilityState={{ busy: isBusy, expanded: isMenuOpen }}
      disabled={isBusy}
      onPress={handleActionPress}
      style={({ pressed }) => [styles.actionButton, { opacity: isBusy ? 0.6 : pressed ? 0.78 : 1 }]}
    >
      {isUploading ? <ActivityIndicator color={colorPalette.contrastText} /> : null}
      <Text style={[styles.actionButtonText, { color: colorPalette.contrastText }]}>+ {actionLabel}</Text>
    </Pressable>
  ), [actionLabel, colorPalette.contrastText, handleActionPress, isBusy, isMenuOpen, isUploading]);

  const actionButton: ReactNode = useLiquidGlass ? (
    <GlassView isInteractive style={styles.glassButton}>{actionContent}</GlassView>
  ) : isIos ? (
    <BlurView intensity={70} tint="systemMaterial" style={styles.glassButton}>{actionContent}</BlurView>
  ) : (
    <View style={[styles.androidFab, { backgroundColor: colorPalette.primary }]}>{actionContent}</View>
  );

  return (
    <RoleGate allowedRoles={allowedRoles}>
      <View style={[styles.container, { bottom: bottom + 12, position: "absolute", right: 16 }]}>
        {actionButton}
      </View>
      <Modal
        animationType="none"
        transparent
        visible={!isIos && isMenuOpen}
        onRequestClose={() => setIsMenuOpen(false)}
      >
        <View style={styles.modalBackdrop}>
          <Pressable accessibilityLabel="Close upload menu" style={styles.modalDismiss} onPress={() => setIsMenuOpen(false)} />
          <Animated.View entering={FadeInDown.duration(180)} exiting={FadeOutDown.duration(130)} style={[styles.panel, { backgroundColor: colorPalette.background, borderColor: `${colorPalette.secondary}55`, paddingBottom: bottom + 16 }]}>
            <Text style={[styles.panelTitle, { color: colorPalette.text }]}>{classification?.title ?? `Add ${label}`}</Text>
            {classification ? (
              <View style={styles.options}>
                {classification.options.map((option) => {
                  const selected = selectedClassification === option.value;
                  return (
                    <Pressable
                      key={option.value}
                      accessibilityRole="button"
                      accessibilityState={{ selected }}
                      disabled={isBusy}
                      onPress={() => { selectClassification(option.value); haptic(); }}
                      style={({ pressed }) => [styles.option, { backgroundColor: selected ? colorPalette.primary : `${colorPalette.primary}11`, borderColor: selected ? colorPalette.primary : `${colorPalette.secondary}66`, opacity: pressed ? 0.75 : 1 }]}
                    >
                      <Text style={[styles.optionText, { color: selected ? colorPalette.contrastText : colorPalette.text }]}>{option.label}</Text>
                    </Pressable>
                  );
                })}
              </View>
            ) : null}
            {sourceOptions.map((option) => (
              <Pressable
                key={option.source}
                accessibilityRole="button"
                accessibilityState={{ disabled: !canChooseSource || isBusy }}
                disabled={!canChooseSource || isBusy}
                onPress={() => handleSource(option.source)}
                style={({ pressed }) => [styles.sourceButton, { borderColor: `${colorPalette.secondary}77`, opacity: !canChooseSource || isBusy ? 0.45 : pressed ? 0.72 : 1 }]}
              >
                {activeSource === option.source ? <ActivityIndicator color={colorPalette.primary} /> : null}
                <Text style={[styles.sourceText, { color: colorPalette.text }]}>{option.label}</Text>
              </Pressable>
            ))}
            {retryAsset ? <Text accessibilityRole="alert" style={[styles.retryHint, { color: colorPalette.secondary }]}>Your last upload can be retried from the error prompt.</Text> : null}
          </Animated.View>
        </View>
      </Modal>
    </RoleGate>
  );
};

export default SiteUploadAction;
