import Ionicons from "@expo/vector-icons/Ionicons";
import { VideoView, useVideoPlayer } from "expo-video";
import { useState } from "react";
import { ActivityIndicator, Image, Linking, Modal, Pressable, ScrollView, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { formatIssueDate, formatIssueDateTime, formatIssueText } from "../formatters";
import { useGetIssueById } from "../hooks/useGetIssueById";
import { useGetIssueAttachments, useIssueAttachmentAccess } from "../hooks/useIssueAttachments";
import type { IssueAttachment, IssueAttachmentKind, SiteIssue } from "../types";
import styles from "./IssueDetailsPage.styles";

type Viewer = { kind: IssueAttachmentKind; name: string; url: string } | null;

const formatSize = (size: number) => size >= 1024 * 1024 ? `${(size / 1024 / 1024).toFixed(1)} MB` : `${Math.ceil(size / 1024)} KB`;
const workerDisplayName = (worker: SiteIssue["assignedWorkers"][number]) => worker.userName?.trim() || worker.email?.trim() || worker.id;

const VideoViewer = ({ url }: { url: string }) => {
  const player = useVideoPlayer({ uri: url }, (videoPlayer) => videoPlayer.play());
  return <VideoView contentFit="contain" nativeControls player={player} style={styles.viewerVideo} />;
};

const AttachmentViewer = ({ viewer, onClose }: { viewer: Viewer; onClose: () => void }) => {
  if (!viewer) return null;
  return <Modal animationType="fade" onRequestClose={onClose} presentationStyle="fullScreen" visible>
    <View style={styles.viewer}>
      <View style={styles.viewerHeader}><Text numberOfLines={1} style={styles.viewerTitle}>{viewer.name}</Text><Pressable accessibilityLabel="Close attachment viewer" accessibilityRole="button" onPress={onClose}><Ionicons color="#fff" name="close" size={28} /></Pressable></View>
      {viewer.kind === "Image" ? <Image resizeMode="contain" source={{ uri: viewer.url }} style={styles.viewerImage} /> : <VideoViewer url={viewer.url} />}
    </View>
  </Modal>;
};

const IssueDetailsPage = () => {
  const colorPalette = useColorPalette();
  const { issueId } = useGetSearchParams<{ issueId?: string }>();
  const [activeTab, setActiveTab] = useState<"details" | "attachments">("details");
  const [viewer, setViewer] = useState<Viewer>(null);
  const [openError, setOpenError] = useState<string | null>(null);
  const issueQuery = useGetIssueById({ issueId });
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

  const issue = issueQuery.data;
  const sections: readonly { title: string; items: readonly { label: string; value: string }[] }[] = issue ? [
    { title: "Issue", items: [{ label: "Reference", value: `#${issue.numberId}` }, { label: "Status", value: issue.status }, { label: "Description", value: issue.description }, { label: "Start date", value: formatIssueDate(issue.startDate) }, { label: "End date", value: formatIssueDate(issue.endDate) }] },
    { title: "Assigned workers", items: [{ label: "Workers", value: issue.assignedWorkers.length ? issue.assignedWorkers.map(workerDisplayName).join(", ") : "—" }] },
    { title: "Activity", items: [{ label: "Created", value: formatIssueDateTime(issue.created) }, { label: "Created by", value: formatIssueText(issue.createdBy) }, { label: "Last modified", value: formatIssueDateTime(issue.lastModified) }, { label: "Last modified by", value: formatIssueText(issue.lastModifiedBy) }] },
  ] : [];

  return <>
    <ScrollView contentContainerStyle={styles.content} contentInsetAdjustmentBehavior="automatic">
      <View style={styles.tabs}>{(["details", "attachments"] as const).map((tab) => <Pressable key={tab} accessibilityRole="tab" accessibilityState={{ selected: activeTab === tab }} onPress={() => setActiveTab(tab)} style={({ pressed }) => [styles.tab, { borderColor: colorPalette.primary, backgroundColor: activeTab === tab ? colorPalette.primary : colorPalette.background, opacity: pressed ? 0.78 : 1 }]}><Text style={[styles.tabText, { color: activeTab === tab ? colorPalette.contrastText : colorPalette.text }]}>{tab === "details" ? "Details" : "Attachments"}</Text></Pressable>)}</View>
      {activeTab === "details" ? issueQuery.isLoading ? <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><ActivityIndicator color={colorPalette.primary} /><Text style={[styles.stateText, { color: colorPalette.secondary }]}>Loading issue details…</Text></View> : issue ? sections.map((section) => <View key={section.title} style={styles.section}><Text style={[styles.sectionTitle, { color: colorPalette.text }]}>{section.title}</Text>{section.items.map((item) => <View key={item.label} style={[styles.detailRow, { borderBottomColor: `${colorPalette.secondary}44` }]}><Text style={[styles.label, { color: colorPalette.secondary }]}>{item.label}</Text><Text selectable style={[styles.value, { color: colorPalette.text }]}>{item.value}</Text></View>)}</View>) : <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.text }]}>{issueQuery.error instanceof Error ? issueQuery.error.message : "Issue details could not be retrieved."}</Text></View> : attachmentsQuery.isLoading ? <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><ActivityIndicator color={colorPalette.primary} /><Text style={[styles.stateText, { color: colorPalette.secondary }]}>Loading attachments…</Text></View> : attachmentsQuery.isError ? <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.text }]}>{attachmentsQuery.error instanceof Error ? attachmentsQuery.error.message : "Attachments could not be retrieved."}</Text></View> : attachmentsQuery.data?.length ? attachmentsQuery.data.map((attachment) => <View key={attachment.id} style={[styles.attachment, { borderColor: `${colorPalette.secondary}55` }]}><Ionicons color={colorPalette.primary} name={attachment.kind === "Image" ? "image" : attachment.kind === "Video" ? "videocam" : "document"} size={25} /><View style={styles.attachmentInfo}><Text numberOfLines={1} style={[styles.attachmentName, { color: colorPalette.text }]}>{attachment.fileName}</Text><Text style={[styles.attachmentMeta, { color: colorPalette.secondary }]}>{attachment.kind} · {formatSize(attachment.sizeBytes)}{attachment.durationSeconds ? ` · ${attachment.durationSeconds}s` : ""}</Text></View><Pressable accessibilityLabel={`Open ${attachment.fileName}`} accessibilityRole="button" disabled={access.isPending} onPress={() => void openAttachment(attachment)} style={({ pressed }) => [styles.openButton, { backgroundColor: `${colorPalette.primary}18`, opacity: access.isPending ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[styles.openButtonText, { color: colorPalette.primary }]}>{attachment.kind === "File" ? "Open" : "View"}</Text></Pressable></View>) : <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.secondary }]}>No attachments yet.</Text></View>}
      {openError ? <Text accessibilityRole="alert" style={{ color: "#B42318" }}>{openError}</Text> : null}
    </ScrollView>
    <AttachmentViewer onClose={() => setViewer(null)} viewer={viewer} />
  </>;
};

export default IssueDetailsPage;
