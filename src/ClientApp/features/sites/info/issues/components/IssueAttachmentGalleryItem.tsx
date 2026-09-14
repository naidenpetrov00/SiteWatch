import Ionicons from "@expo/vector-icons/Ionicons";
import { Image } from "expo-image";
import { memo, useCallback } from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetIssueAttachmentPreview } from "../hooks/useIssueAttachments";
import type { IssueAttachmentKind } from "../types";
import styles from "./IssueAttachmentGallery.styles";

type IssueAttachmentGalleryItemProps = {
  attachmentId: string;
  contentType: string;
  durationSeconds: number | null;
  fileName: string;
  hasPreview: boolean;
  isOpening: boolean;
  issueId?: string;
  kind: IssueAttachmentKind;
  onOpen: (attachmentId: string) => void;
};

const formatDuration = (durationSeconds: number | null) => {
  if (durationSeconds == null || Number.isNaN(durationSeconds)) return "0:00";

  const totalSeconds = Math.max(0, Math.round(durationSeconds));
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = (totalSeconds % 60).toString().padStart(2, "0");
  return hours > 0 ? `${hours}:${minutes.toString().padStart(2, "0")}:${seconds}` : `${minutes}:${seconds}`;
};

const IssueAttachmentGalleryItem = ({
  attachmentId,
  contentType,
  durationSeconds,
  fileName,
  hasPreview,
  isOpening,
  issueId,
  kind,
  onOpen,
}: IssueAttachmentGalleryItemProps) => {
  const colorPalette = useColorPalette();
  const previewQuery = useGetIssueAttachmentPreview(issueId, attachmentId, hasPreview);
  const previewUrl = previewQuery.data?.url;
  const handlePress = useCallback(() => onOpen(attachmentId), [attachmentId, onOpen]);
  const hasMediaPreview = (kind === "Image" || kind === "Video") && Boolean(previewUrl);
  const iconName = kind === "Image" ? "image" : kind === "Video" ? "videocam" : "document";

  return (
    <Pressable
      accessibilityLabel={`Open ${fileName}`}
      accessibilityRole="button"
      disabled={isOpening}
      onPress={handlePress}
      style={({ pressed }) => [
        styles.tile,
        { backgroundColor: `${colorPalette.primary}18`, opacity: isOpening ? 0.55 : pressed ? 0.82 : 1 },
      ]}
    >
      {hasMediaPreview ? (
        <Image
          cachePolicy="none"
          contentFit="cover"
          recyclingKey={attachmentId}
          source={{ uri: previewUrl }}
          style={styles.image}
          transition={150}
        />
      ) : hasPreview && previewQuery.isLoading && kind !== "File" ? (
        <View style={styles.placeholder}><ActivityIndicator color={colorPalette.primary} /></View>
      ) : (
        <View style={styles.placeholder}>
          <Ionicons color={colorPalette.primary} name={iconName} size={32} />
          {kind === "File" ? <><Text numberOfLines={2} style={[styles.fileName, { color: colorPalette.text }]}>{fileName}</Text><Text numberOfLines={1} style={[styles.fileType, { color: colorPalette.secondary }]}>{contentType}</Text></> : null}
        </View>
      )}
      {kind === "Video" ? <View pointerEvents="none" style={styles.durationOverlay}><View style={styles.durationBadge}><Text style={styles.durationText}>{formatDuration(durationSeconds)}</Text></View></View> : null}
    </Pressable>
  );
};

export default memo(IssueAttachmentGalleryItem);
