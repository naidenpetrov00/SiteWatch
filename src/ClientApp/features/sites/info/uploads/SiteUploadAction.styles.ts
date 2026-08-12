import { StyleSheet } from "react-native";

const styles = StyleSheet.create({
  container: { alignItems: "flex-end" },
  glassButton: { borderCurve: "continuous", borderRadius: 28, overflow: "hidden" },
  androidFab: { borderRadius: 28, boxShadow: "0 8px 20px rgba(0, 0, 0, 0.22)" },
  actionButton: { alignItems: "center", flexDirection: "row", gap: 8, justifyContent: "center", minHeight: 56, minWidth: 56, paddingHorizontal: 20, paddingVertical: 12 },
  actionButtonText: { fontSize: 15, fontWeight: "700" },
  modalBackdrop: { backgroundColor: "rgba(0, 0, 0, 0.35)", flex: 1, justifyContent: "flex-end" },
  modalDismiss: { flex: 1 },
  panel: { borderCurve: "continuous", borderTopLeftRadius: 28, borderTopRightRadius: 28, borderWidth: 1, gap: 12, padding: 20 },
  panelTitle: { fontSize: 17, fontWeight: "700" },
  options: { flexDirection: "row", flexWrap: "wrap", gap: 8 },
  option: { borderCurve: "continuous", borderRadius: 999, borderWidth: 1, paddingHorizontal: 14, paddingVertical: 10 },
  optionText: { fontSize: 14, fontWeight: "600" },
  sourceButton: { alignItems: "center", borderCurve: "continuous", borderRadius: 16, borderWidth: 1, flexDirection: "row", gap: 10, minHeight: 52, paddingHorizontal: 16, paddingVertical: 12 },
  sourceText: { fontSize: 16, fontWeight: "600" },
  retryHint: { fontSize: 13 },
});

export default styles;
