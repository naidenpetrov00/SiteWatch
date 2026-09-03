import Ionicons from "@expo/vector-icons/Ionicons";
import { VideoView, useVideoPlayer } from "expo-video";
import { Image, Linking, Pressable, ScrollView, Text, TextInput, View } from "react-native";
import { useEffect, useState } from "react";

import { useColorPalette } from "@/hooks/useColorPalette";
import ConnectedTabs from "@/components/ui/ConnectedTabs";
import {
  selectUploadAssets,
  type PickedUploadAsset,
  type UploadSource,
} from "@/features/sites/info/uploads/selectUploadAssets";
import { useCreateIssue } from "../hooks/useCreateIssue";
import { useUploadIssueAttachment } from "../hooks/useIssueAttachments";
import type { IssueAttachmentKind, PendingIssueAttachment } from "../types";
import addIssueModalStyles from "./AddIssueModal.styles";

type AddIssueFormProps = { siteId: string; visible: boolean; onClose: () => void };

const IMAGE_TYPES = new Set(["image/jpeg", "image/png", "image/webp", "image/gif", "image/heic", "image/heif"]);
const VIDEO_TYPES = new Set(["video/mp4", "video/quicktime", "video/webm"]);

const getErrorMessage = (error: unknown) =>
  error instanceof Error && error.message.trim() ? error.message : "Unable to add the issue. Please try again.";

const contentTypeFromName = (fileName: string) => {
  const extension = fileName.split(".").pop()?.toLowerCase();
  const types: Record<string, string> = {
    gif: "image/gif", heic: "image/heic", heif: "image/heif", jpeg: "image/jpeg", jpg: "image/jpeg",
    mov: "video/quicktime", mp4: "video/mp4", png: "image/png", webm: "video/webm", webp: "image/webp",
  };
  return extension ? types[extension] ?? "application/octet-stream" : "application/octet-stream";
};

const fallbackFileName = (asset: PickedUploadAsset, index: number) => {
  if (asset.fileName?.trim()) return asset.fileName.trim();
  const prefix = asset.mediaType === "video" ? "video" : "image";
  const extension = asset.mediaType === "video" ? "mp4" : "jpg";
  return `${prefix}-attachment-${Date.now()}-${index}.${extension}`;
};

const fallbackContentType = (asset: PickedUploadAsset, fileName: string) => {
  if (asset.mimeType) return asset.mimeType;
  const fromName = contentTypeFromName(fileName);
  if (fromName !== "application/octet-stream") return fromName;
  if (asset.mediaType === "video") return "video/mp4";
  if (asset.mediaType === "image") return "image/jpeg";
  return fromName;
};

const validateAttachment = (asset: { fileName: string; contentType: string; fileSize?: number | null }): { kind: IssueAttachmentKind; contentType: string } | string => {
  const fileName = asset.fileName.trim();
  let contentType = asset.contentType.trim().toLowerCase();
  if (contentType === "image/jpg") contentType = "image/jpeg";
  if (!fileName || fileName.length > 512) return "The file name is required and must be at most 512 characters.";
  if (contentType.length > 128) return "The file content type must be at most 128 characters.";
  if (asset.fileSize === 0) return "The file cannot be empty.";
  const kind: IssueAttachmentKind = IMAGE_TYPES.has(contentType) ? "Image" : VIDEO_TYPES.has(contentType) ? "Video" : "File";
  if (contentType.startsWith("image/") && kind !== "Image") return "Only JPEG, PNG, WebP, GIF, HEIC, or HEIF images are supported.";
  if (contentType.startsWith("video/") && kind !== "Video") return "Only MP4, MOV, or WebM videos are supported.";
  const maximum = kind === "Image" ? 50 * 1024 * 1024 : kind === "Video" ? 500 * 1024 * 1024 : 100 * 1024 * 1024;
  if (asset.fileSize !== undefined && asset.fileSize !== null && asset.fileSize > maximum) return `The ${kind.toLowerCase()} cannot exceed ${maximum / 1024 / 1024} MB.`;
  return { kind, contentType };
};

const formatSize = (size?: number) => size === undefined ? "Unknown size" : `${(size / 1024 / 1024).toFixed(size >= 1024 * 1024 ? 1 : 2)} MB`;

const QueuedVideoPreview = ({ uri }: { uri: string }) => {
  const player = useVideoPlayer({ uri });
  return <VideoView contentFit="cover" player={player} style={addIssueModalStyles.attachmentPreview} />;
};

const AddIssueForm = ({ siteId, visible, onClose }: AddIssueFormProps) => {
  const colorPalette = useColorPalette();
  const createIssue = useCreateIssue();
  const uploadAttachment = useUploadIssueAttachment();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<"details" | "attachments">("details");
  const [attachments, setAttachments] = useState<PendingIssueAttachment[]>([]);
  const [persistedIssueId, setPersistedIssueId] = useState<string | null>(null);
  const [activePicker, setActivePicker] = useState<UploadSource | null>(null);
  const [cameraPermissionDenied, setCameraPermissionDenied] = useState(false);

  useEffect(() => {
    if (!visible) return;
    setTitle(""); setDescription(""); setError(null); setActiveTab("details"); setAttachments([]); setPersistedIssueId(null); setActivePicker(null); setCameraPermissionDenied(false);
  }, [visible]);

  const addPickedAttachments = (assets: PickedUploadAsset[]) => {
    const accepted: PendingIssueAttachment[] = [];
    const errors: string[] = [];
    for (const [index, asset] of assets.entries()) {
      const fileName = fallbackFileName(asset, index);
      const validation = validateAttachment({
        fileName,
        contentType: fallbackContentType(asset, fileName),
        fileSize: asset.fileSize,
      });
      if (typeof validation === "string") { errors.push(`${fileName}: ${validation}`); continue; }
      accepted.push({ clientId: `issue-attachment-${Date.now()}-${index}`, uri: asset.uri, fileName, contentType: validation.contentType, fileSize: asset.fileSize ?? undefined, kind: validation.kind, status: "queued", error: null });
    }
    if (accepted.length) setAttachments((current) => [...current, ...accepted]);
    setError(errors.length ? errors.join(" ") : null);
  };

  const chooseAttachments = async (source: UploadSource) => {
    if (persistedIssueId) return;
    setActivePicker(source);
    setCameraPermissionDenied(false);
    try {
      const result = await selectUploadAssets({
        source,
        mediaKind: "media",
        documentPickerTypes: "*/*",
        multiple: source !== "camera",
        imageQuality: source === "camera" ? 0.9 : 1,
      });
      if (result.status === "permission-denied") {
        setCameraPermissionDenied(true);
        setError("Camera access is required to capture an issue attachment. Allow Camera access in Settings and try again.");
        return;
      }
      if (result.status === "selected") addPickedAttachments(result.assets);
    } catch (pickerError) {
      setError(getErrorMessage(pickerError));
    } finally {
      setActivePicker(null);
    }
  };

  const submit = async () => {
    const trimmedTitle = title.trim();
    const trimmedDescription = description.trim();
    if (!trimmedTitle || trimmedTitle.length > 200) { setActiveTab("details"); setError("Enter an issue title of up to 200 characters."); return; }
    if (!trimmedDescription || trimmedDescription.length > 4000) { setActiveTab("details"); setError("Enter an issue description of up to 4,000 characters."); return; }
    try {
      const issueId = persistedIssueId ?? (await createIssue.mutateAsync({ siteId, title: trimmedTitle, description: trimmedDescription })).id;
      setPersistedIssueId(issueId);
      let failed = false;
      for (const attachment of attachments.filter(({ status }) => status === "queued" || status === "error")) {
        setAttachments((current) => current.map((item) => item.clientId === attachment.clientId ? { ...item, status: "uploading", error: null } : item));
        try {
          await uploadAttachment.mutateAsync({ issueId, attachment });
          setAttachments((current) => current.filter((item) => item.clientId !== attachment.clientId));
        } catch (uploadError) {
          failed = true;
          setAttachments((current) => current.map((item) => item.clientId === attachment.clientId ? { ...item, status: "error", error: getErrorMessage(uploadError) } : item));
        }
      }
      if (failed) { setActiveTab("attachments"); setError("The issue was created, but some attachments could not be uploaded. Retry them before leaving this screen."); return; }
      onClose();
    } catch (submissionError) { setError(getErrorMessage(submissionError)); }
  };

  const isSaving = createIssue.isPending || uploadAttachment.isPending;
  const isPickerBusy = activePicker !== null;

  return <>
    <ScrollView contentContainerStyle={addIssueModalStyles.content} contentInsetAdjustmentBehavior="automatic">
      <Text style={[addIssueModalStyles.title, { color: colorPalette.text }]}>{persistedIssueId ? "Finish attachments" : "Add Issue"}</Text>
      <Text style={[addIssueModalStyles.subtitle, { color: colorPalette.secondary }]}>{persistedIssueId ? "Retry failed attachments before leaving this screen." : "Describe a problem for this site. New issues are created with an Open status."}</Text>
      <ConnectedTabs onValueChange={(value) => setActiveTab(value as "details" | "attachments")} tabs={[
        { label: "Details", value: "details" },
        { label: `Attachments${attachments.length ? ` (${attachments.length})` : ""}`, value: "attachments" },
      ]} value={activeTab}>
      {activeTab === "details" ? <>
        <View style={addIssueModalStyles.field}><Text style={[addIssueModalStyles.label, { color: colorPalette.text }]}>Title</Text><TextInput accessibilityLabel="Issue title" autoFocus={!persistedIssueId} editable={!persistedIssueId} maxLength={200} onChangeText={(value) => { setTitle(value); setError(null); }} placeholder="Describe the issue" placeholderTextColor={colorPalette.secondary} style={[addIssueModalStyles.input, { borderColor: colorPalette.secondary, color: colorPalette.text }]} value={title} /></View>
        <View style={addIssueModalStyles.field}><Text style={[addIssueModalStyles.label, { color: colorPalette.text }]}>Description</Text><TextInput accessibilityLabel="Issue description" editable={!persistedIssueId} maxLength={4000} multiline onChangeText={(value) => { setDescription(value); setError(null); }} placeholder="Add the relevant details" placeholderTextColor={colorPalette.secondary} style={[addIssueModalStyles.input, addIssueModalStyles.descriptionInput, { borderColor: colorPalette.secondary, color: colorPalette.text }]} value={description} /></View>
      </> : <View style={addIssueModalStyles.attachmentActions}>
        {!persistedIssueId ? <View style={addIssueModalStyles.attachmentSourceActions}>
          <Pressable accessibilityRole="button" accessibilityState={{ busy: activePicker === "gallery" }} disabled={isPickerBusy} onPress={() => void chooseAttachments("gallery")} style={({ pressed }) => [addIssueModalStyles.chooseFilesButton, { borderColor: colorPalette.primary, opacity: isPickerBusy ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[addIssueModalStyles.chooseFilesText, { color: colorPalette.primary }]}>{activePicker === "gallery" ? "Opening gallery…" : "Upload from gallery"}</Text></Pressable>
          <Pressable accessibilityRole="button" accessibilityState={{ busy: activePicker === "camera" }} disabled={isPickerBusy} onPress={() => void chooseAttachments("camera")} style={({ pressed }) => [addIssueModalStyles.chooseFilesButton, { borderColor: colorPalette.primary, opacity: isPickerBusy ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[addIssueModalStyles.chooseFilesText, { color: colorPalette.primary }]}>{activePicker === "camera" ? "Opening camera…" : "Use camera"}</Text></Pressable>
          <Pressable accessibilityRole="button" accessibilityState={{ busy: activePicker === "file" }} disabled={isPickerBusy} onPress={() => void chooseAttachments("file")} style={({ pressed }) => [addIssueModalStyles.chooseFilesButton, { borderColor: colorPalette.primary, opacity: isPickerBusy ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[addIssueModalStyles.chooseFilesText, { color: colorPalette.primary }]}>{activePicker === "file" ? "Opening files…" : "Choose files"}</Text></Pressable>
        </View> : null}
        <Text style={[addIssueModalStyles.attachmentHint, { color: colorPalette.secondary }]}>Images up to 50 MB, videos up to 500 MB, and other files up to 100 MB.</Text>
        {attachments.length ? <View style={addIssueModalStyles.attachmentList}>{attachments.map((attachment) => <View key={attachment.clientId} style={[addIssueModalStyles.attachmentRow, { borderColor: `${colorPalette.secondary}55` }]}>{attachment.kind === "Image" ? <Image source={{ uri: attachment.uri }} style={addIssueModalStyles.attachmentPreview} /> : attachment.kind === "Video" ? <QueuedVideoPreview uri={attachment.uri} /> : <View style={[addIssueModalStyles.attachmentIcon, { backgroundColor: `${colorPalette.primary}18` }]}><Ionicons color={colorPalette.primary} name="document" size={22} /></View>}<View style={addIssueModalStyles.attachmentInfo}><Text numberOfLines={1} style={[addIssueModalStyles.attachmentName, { color: colorPalette.text }]}>{attachment.fileName}</Text><Text style={[addIssueModalStyles.attachmentMetadata, { color: attachment.error ? "#B42318" : colorPalette.secondary }]}>{attachment.error ?? `${attachment.kind} · ${formatSize(attachment.fileSize)}${attachment.status === "uploading" ? " · Uploading…" : ""}`}</Text></View>{!persistedIssueId ? <Pressable accessibilityLabel={`Remove ${attachment.fileName}`} accessibilityRole="button" onPress={() => setAttachments((current) => current.filter((item) => item.clientId !== attachment.clientId))} style={addIssueModalStyles.removeAttachment}><Ionicons color={colorPalette.secondary} name="close" size={20} /></Pressable> : null}</View>)}</View> : <View style={[addIssueModalStyles.emptyAttachments, { borderColor: `${colorPalette.secondary}55` }]}><Text style={{ color: colorPalette.secondary }}>No attachments selected.</Text></View>}
      </View>}
      </ConnectedTabs>
      {error ? <View style={[addIssueModalStyles.error, { borderColor: "#B42318" }]}><Text accessibilityRole="alert" style={{ color: "#B42318", fontSize: 14 }}>{error}</Text>{cameraPermissionDenied ? <Pressable accessibilityRole="button" onPress={() => void Linking.openSettings()} style={addIssueModalStyles.errorAction}><Text style={[addIssueModalStyles.errorActionText, { color: colorPalette.primary }]}>Open Settings</Text></Pressable> : null}</View> : null}
    </ScrollView>
    <View style={[addIssueModalStyles.footer, { backgroundColor: colorPalette.background }]}>
      <Pressable accessibilityRole="button" disabled={isSaving} onPress={onClose} style={({ pressed }) => [addIssueModalStyles.button, { backgroundColor: colorPalette.secondary, opacity: isSaving ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[addIssueModalStyles.buttonText, { color: colorPalette.background }]}>Cancel</Text></Pressable>
      <Pressable accessibilityRole="button" accessibilityState={{ busy: isSaving }} disabled={isSaving} onPress={() => void submit()} style={({ pressed }) => [addIssueModalStyles.button, { backgroundColor: colorPalette.primary, opacity: isSaving ? 0.55 : pressed ? 0.78 : 1 }]}><Text style={[addIssueModalStyles.buttonText, { color: colorPalette.background }]}>{isSaving ? "Saving…" : persistedIssueId ? "Retry attachments" : "Add Issue"}</Text></Pressable>
    </View>
  </>;
};

export default AddIssueForm;
