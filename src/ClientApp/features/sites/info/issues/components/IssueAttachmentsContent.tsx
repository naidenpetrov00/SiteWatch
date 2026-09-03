import { useCallback, useMemo, useState } from "react";
import { ActivityIndicator, FlatList, Linking, Text, useWindowDimensions, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { useGetIssueAttachments, useIssueAttachmentAccess } from "../hooks/useIssueAttachments";
import type { IssueAttachment } from "../types";
import IssueAttachmentViewer, { type AttachmentViewerState } from "./IssueAttachmentViewer";
import IssueAttachmentGalleryItem from "./IssueAttachmentGalleryItem";
import { ATTACHMENT_GRID_GAP, ATTACHMENT_GRID_HORIZONTAL_PADDING } from "./IssueAttachmentGallery.styles";
import styles from "./IssueDetailsPage.styles";

const MIN_TILE_WIDTH = 150;

const IssueAttachmentsContent = () => {
  const colorPalette = useColorPalette();
  const { bottom } = useSafeAreaInsets();
  const { width } = useWindowDimensions();
  const { issueId } = useGetSearchParams<{ issueId?: string }>();
  const [viewer, setViewer] = useState<AttachmentViewerState>(null);
  const [openError, setOpenError] = useState<string | null>(null);
  const attachmentsQuery = useGetIssueAttachments(issueId);
  const access = useIssueAttachmentAccess();
  const attachments = attachmentsQuery.data ?? [];
  const numColumns = Math.max(1, Math.floor((width - ATTACHMENT_GRID_HORIZONTAL_PADDING * 2 + ATTACHMENT_GRID_GAP) / (MIN_TILE_WIDTH + ATTACHMENT_GRID_GAP)));
  const tileWidth = (width - ATTACHMENT_GRID_HORIZONTAL_PADDING * 2 - ATTACHMENT_GRID_GAP * (numColumns - 1)) / numColumns;

  const openAttachment = useCallback(async (attachment: IssueAttachment) => {
    if (!issueId) return;
    setOpenError(null);
    try {
      const result = await access.mutateAsync({ issueId, attachmentId: attachment.id, download: attachment.kind === "File" });
      if (attachment.kind === "File") await Linking.openURL(result.url);
      else setViewer({ kind: attachment.kind, name: attachment.fileName, url: result.url });
    } catch (error) {
      setOpenError(error instanceof Error ? error.message : `Unable to open ${attachment.fileName}.`);
    }
  }, [access, issueId]);

  const handleOpenAttachment = useCallback((attachmentId: string) => {
    const attachment = attachments.find(({ id }) => id === attachmentId);
    if (attachment) void openAttachment(attachment);
  }, [attachments, openAttachment]);

  const renderAttachment = useCallback(({ item }: { item: IssueAttachment }) => (
    <View style={{ width: tileWidth }}>
      <IssueAttachmentGalleryItem
        attachmentId={item.id}
        contentType={item.contentType}
        durationSeconds={item.durationSeconds}
        fileName={item.fileName}
        hasPreview={item.hasPreview}
        isOpening={access.isPending}
        issueId={issueId}
        kind={item.kind}
        onOpen={handleOpenAttachment}
      />
    </View>
  ), [access.isPending, handleOpenAttachment, issueId, tileWidth]);

  const emptyState = useMemo(() => {
    if (attachmentsQuery.isLoading) return <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><ActivityIndicator color={colorPalette.primary} /><Text style={[styles.stateText, { color: colorPalette.secondary }]}>Loading attachments…</Text></View>;
    if (attachmentsQuery.isError) return <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.text }]}>{attachmentsQuery.error instanceof Error ? attachmentsQuery.error.message : "Attachments could not be retrieved."}</Text></View>;
    return <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.secondary }]}>No attachments yet.</Text></View>;
  }, [attachmentsQuery.error, attachmentsQuery.isError, attachmentsQuery.isLoading, colorPalette]);

  return <><FlatList
    columnWrapperStyle={numColumns > 1 ? { gap: ATTACHMENT_GRID_GAP } : undefined}
    contentContainerStyle={[{ gap: ATTACHMENT_GRID_GAP, padding: ATTACHMENT_GRID_HORIZONTAL_PADDING, paddingBottom: bottom + 80 }, attachments.length === 0 ? { flexGrow: 1 } : null]}
    contentInsetAdjustmentBehavior="automatic"
    data={attachments}
    key={numColumns}
    keyExtractor={(attachment) => attachment.id}
    ListEmptyComponent={emptyState}
    ListFooterComponent={openError ? <Text accessibilityRole="alert" style={styles.errorText}>{openError}</Text> : null}
    numColumns={numColumns}
    renderItem={renderAttachment}
    showsVerticalScrollIndicator={false}
  /><IssueAttachmentViewer onClose={() => setViewer(null)} viewer={viewer} /></>;
};

export default IssueAttachmentsContent;
