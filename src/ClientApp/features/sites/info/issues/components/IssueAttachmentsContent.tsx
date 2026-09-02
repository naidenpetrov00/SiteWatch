import Ionicons from "@expo/vector-icons/Ionicons";
import { useState } from "react";
import { ActivityIndicator, Linking, Pressable, ScrollView, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useGetIssueAttachments, useIssueAttachmentAccess } from "../hooks/useIssueAttachments";
import type { IssueAttachment } from "../types";
import IssueAttachmentViewer, { type AttachmentViewerState } from "./IssueAttachmentViewer";
import styles from "./IssueDetailsPage.styles";

const formatSize = (size: number) => size >= 1024 * 1024 ? `${(size / 1024 / 1024).toFixed(1)} MB` : `${Math.ceil(size / 1024)} KB`;

const IssueAttachmentsContent = () => {
  const colorPalette = useColorPalette();
  const { issueId } = useGetSearchParams<{ issueId?: string }>();
  const [viewer, setViewer] = useState<AttachmentViewerState>(null);
  const [openError, setOpenError] = useState<string | null>(null);
  const attachmentsQuery = useGetIssueAttachments(issueId);
  const access = useIssueAttachmentAccess();

  const openAttachment = async (attachment: IssueAttachment) => {
    if (!issueId) return;
    setOpenError(null);
    try {
      const result = await access.mutateAsync({ issueId, attachmentId: attachment.id, download: attachment.kind === "File" });
      if (attachment.kind === "File") await Linking.openURL(result.url);
      else setViewer({ kind: attachment.kind, name: attachment.fileName, url: result.url });
    } catch (error) {
      setOpenError(error instanceof Error ? error.message : `Unable to open ${attachment.fileName}.`);
    }
  };

  return <><ScrollView contentContainerStyle={styles.content} contentInsetAdjustmentBehavior="automatic">
    {attachmentsQuery.isLoading ? <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><ActivityIndicator color={colorPalette.primary} /><Text style={[styles.stateText, { color: colorPalette.secondary }]}>Loading attachments…</Text></View> : attachmentsQuery.isError ? <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.text }]}>{attachmentsQuery.error instanceof Error ? attachmentsQuery.error.message : "Attachments could not be retrieved."}</Text></View> : attachmentsQuery.data?.length ? attachmentsQuery.data.map((attachment) => <View key={attachment.id} style={[styles.attachment, { borderColor: `${colorPalette.secondary}55` }]}><Ionicons color={colorPalette.primary} name={attachment.kind === "Image" ? "image" : attachment.kind === "Video" ? "videocam" : "document"} size={25} /><View style={styles.attachmentInfo}><Text numberOfLines={1} style={[styles.attachmentName, { color: colorPalette.text }]}>{attachment.fileName}</Text><Text style={[styles.attachmentMeta, { color: colorPalette.secondary }]}>{attachment.kind} · {formatSize(attachment.sizeBytes)}{attachment.durationSeconds ? ` · ${attachment.durationSeconds}s` : ""}</Text></View><Pressable accessibilityLabel={`Open ${attachment.fileName}`} accessibilityRole="button" disabled={access.isPending} onPress={() => void openAttachment(attachment)} style={({ pressed }) => [styles.openButton, { backgroundColor: `${colorPalette.primary}18`, opacity: access.isPending ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[styles.openButtonText, { color: colorPalette.primary }]}>{attachment.kind === "File" ? "Open" : "View"}</Text></Pressable></View>) : <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.secondary }]}>No attachments yet.</Text></View>}
    {openError ? <Text accessibilityRole="alert" style={styles.errorText}>{openError}</Text> : null}
  </ScrollView><IssueAttachmentViewer onClose={() => setViewer(null)} viewer={viewer} /></>;
};

export default IssueAttachmentsContent;
