import { StyleSheet } from "react-native";

const styles = StyleSheet.create({
  content: { gap: 16, padding: 20 },
  tabs: { flexDirection: "row", gap: 8 },
  tab: { alignItems: "center", borderRadius: 999, borderWidth: 1, flex: 1, paddingVertical: 9 },
  tabText: { fontSize: 14, fontWeight: "700" },
  section: { gap: 8 },
  sectionTitle: { fontSize: 17, fontWeight: "700" },
  detailRow: { borderBottomWidth: 1, gap: 4, paddingVertical: 10 },
  label: { fontSize: 13, fontWeight: "600" },
  value: { fontSize: 15, lineHeight: 21 },
  state: { alignItems: "center", borderRadius: 12, borderWidth: 1, gap: 8, padding: 20 },
  stateText: { fontSize: 14, textAlign: "center" },
  attachment: { alignItems: "center", borderRadius: 10, borderWidth: 1, flexDirection: "row", gap: 12, padding: 12 },
  attachmentInfo: { flex: 1, gap: 3 },
  attachmentName: { fontSize: 15, fontWeight: "700" },
  attachmentMeta: { fontSize: 12 },
  openButton: { borderRadius: 8, paddingHorizontal: 10, paddingVertical: 7 },
  openButtonText: { fontSize: 13, fontWeight: "700" },
  viewer: { backgroundColor: "#000", flex: 1 },
  viewerImage: { flex: 1 },
  viewerVideo: { flex: 1 },
  viewerHeader: { alignItems: "center", flexDirection: "row", justifyContent: "space-between", padding: 16 },
  viewerTitle: { color: "#fff", flex: 1, fontSize: 16, fontWeight: "700" },
});

export default styles;
