import { StyleSheet } from "react-native";

const addIssueModalStyles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    gap: 16,
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 14,
    lineHeight: 20,
  },
  field: {
    gap: 7,
  },
  label: {
    fontSize: 14,
    fontWeight: "600",
  },
  input: {
    borderRadius: 10,
    borderWidth: 1,
    fontSize: 16,
    paddingHorizontal: 12,
    paddingVertical: 11,
  },
  descriptionInput: {
    minHeight: 150,
    textAlignVertical: "top",
  },
  error: {
    borderRadius: 10,
    borderWidth: 1,
    gap: 8,
    padding: 12,
  },
  attachmentActions: { gap: 8 },
  attachmentSourceActions: { gap: 8 },
  cameraActions: { flexDirection: "row", gap: 8 },
  cameraActionButton: { alignItems: "center", borderRadius: 10, borderWidth: 1, flex: 1, justifyContent: "center", minHeight: 48 },
  chooseFilesButton: { alignItems: "center", borderRadius: 10, borderWidth: 1, paddingVertical: 12 },
  chooseFilesText: { fontSize: 15, fontWeight: "700" },
  errorAction: { alignSelf: "flex-start", paddingVertical: 2 },
  errorActionText: { fontSize: 14, fontWeight: "700" },
  attachmentHint: { fontSize: 13, lineHeight: 18 },
  attachmentList: { gap: 10 },
  attachmentRow: { alignItems: "center", borderRadius: 10, borderWidth: 1, flexDirection: "row", gap: 10, padding: 10 },
  attachmentPreview: { borderRadius: 6, height: 44, width: 44 },
  attachmentIcon: { alignItems: "center", borderRadius: 6, height: 44, justifyContent: "center", width: 44 },
  attachmentInfo: { flex: 1, gap: 2 },
  attachmentName: { fontSize: 14, fontWeight: "700" },
  attachmentMetadata: { fontSize: 12 },
  removeAttachment: { padding: 6 },
  emptyAttachments: { borderRadius: 10, borderWidth: 1, padding: 16 },
  footer: {
    flexDirection: "row",
    gap: 12,
    padding: 20,
  },
  button: {
    alignItems: "center",
    borderRadius: 10,
    flex: 1,
    paddingVertical: 13,
  },
  buttonText: {
    fontSize: 15,
    fontWeight: "700",
  },
});

export default addIssueModalStyles;
