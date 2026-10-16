import { StyleSheet } from "react-native";

const styles = StyleSheet.create({
  screen: { flex: 1 },
  content: { padding: 16, paddingBottom: 48, gap: 14 },
  centered: { flex: 1, alignItems: "center", justifyContent: "center", padding: 24, gap: 12 },
  card: { borderWidth: 1, borderRadius: 16, padding: 16, gap: 8 },
  cardHeader: { flexDirection: "row", justifyContent: "space-between", gap: 12 },
  title: { fontSize: 20, fontWeight: "700", flexShrink: 1 },
  status: { fontSize: 13, fontWeight: "700", textTransform: "uppercase" },
  text: { fontSize: 15, lineHeight: 22 },
  muted: { fontSize: 13, lineHeight: 19 },
  section: { borderWidth: 1, borderRadius: 16, padding: 16, gap: 10 },
  sectionTitle: { fontSize: 18, fontWeight: "700" },
  row: { flexDirection: "row", justifyContent: "space-between", gap: 16, paddingVertical: 5 },
  rowLabel: { flex: 1, fontSize: 14 },
  rowValue: { flex: 1, fontSize: 14, fontWeight: "600", textAlign: "right" },
  item: { paddingVertical: 9, gap: 3, borderBottomWidth: StyleSheet.hairlineWidth },
  actions: { flexDirection: "row", flexWrap: "wrap", gap: 10, marginTop: 6 },
  button: { minHeight: 44, borderRadius: 10, paddingHorizontal: 16, alignItems: "center", justifyContent: "center" },
  buttonText: { fontWeight: "700" },
  secondaryButton: { borderWidth: 1 },
  dangerButton: { backgroundColor: "#b91c1c" },
  warning: { borderRadius: 10, padding: 12, backgroundColor: "#fef3c7" },
  warningText: { color: "#854d0e", fontWeight: "600", lineHeight: 20 },
  error: { color: "#b91c1c", textAlign: "center", lineHeight: 21 },
  modal: { flex: 1, padding: 20, gap: 16 },
  modalTitle: { fontSize: 22, fontWeight: "700" },
  input: { minHeight: 130, borderWidth: 1, borderRadius: 10, padding: 12, textAlignVertical: "top" },
  counter: { textAlign: "right", fontSize: 12 },
});

export default styles;
