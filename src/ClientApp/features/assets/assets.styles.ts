import { StyleSheet } from "react-native";

const assetStyles = StyleSheet.create({
  screen: {
    flex: 1,
  },
  content: {
    gap: 16,
    padding: 20,
    paddingBottom: 120,
  },
  title: {
    fontSize: 28,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 15,
    lineHeight: 21,
  },
  actionGrid: {
    gap: 12,
  },
  actionCard: {
    borderCurve: "continuous",
    borderRadius: 18,
    borderWidth: 1,
    gap: 8,
    padding: 18,
  },
  actionHeader: {
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
  },
  actionTitle: {
    fontSize: 18,
    fontWeight: "700",
  },
  section: {
    borderCurve: "continuous",
    borderRadius: 16,
    borderWidth: 1,
    gap: 10,
    padding: 16,
  },
  sectionTitle: {
    fontSize: 17,
    fontWeight: "700",
  },
  field: {
    gap: 6,
  },
  label: {
    fontSize: 13,
    fontWeight: "700",
  },
  input: {
    borderCurve: "continuous",
    borderRadius: 12,
    borderWidth: 1,
    fontSize: 16,
    minHeight: 48,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  multilineInput: {
    minHeight: 112,
    textAlignVertical: "top",
  },
  errorText: {
    color: "#B42318",
    fontSize: 13,
  },
  errorBox: {
    borderCurve: "continuous",
    borderColor: "#B42318",
    borderRadius: 12,
    borderWidth: 1,
    gap: 10,
    padding: 14,
  },
  buttonRow: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 10,
  },
  button: {
    alignItems: "center",
    borderCurve: "continuous",
    borderRadius: 12,
    justifyContent: "center",
    minHeight: 46,
    paddingHorizontal: 16,
  },
  buttonText: {
    fontSize: 15,
    fontWeight: "700",
  },
  choiceRow: {
    flexDirection: "row",
    gap: 10,
  },
  choice: {
    alignItems: "center",
    borderCurve: "continuous",
    borderRadius: 12,
    borderWidth: 1,
    flex: 1,
    justifyContent: "center",
    minHeight: 48,
    padding: 10,
  },
  choiceText: {
    fontWeight: "700",
    textAlign: "center",
  },
  listContent: {
    gap: 10,
    padding: 16,
    paddingBottom: 120,
  },
  listHeader: {
    gap: 12,
    paddingBottom: 6,
  },
  listItem: {
    borderCurve: "continuous",
    borderRadius: 14,
    borderWidth: 1,
    gap: 4,
    padding: 14,
  },
  listItemTitle: {
    fontSize: 16,
    fontWeight: "700",
  },
  metadata: {
    fontSize: 13,
    lineHeight: 18,
  },
  pagination: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    paddingTop: 12,
  },
  centerState: {
    alignItems: "center",
    flex: 1,
    gap: 14,
    justifyContent: "center",
    padding: 24,
  },
  centerText: {
    lineHeight: 21,
    textAlign: "center",
  },
  camera: {
    flex: 1,
  },
  cameraOverlay: {
    bottom: 0,
    gap: 12,
    left: 0,
    padding: 20,
    position: "absolute",
    right: 0,
  },
  scannerFrame: {
    alignSelf: "center",
    borderCurve: "continuous",
    borderColor: "white",
    borderRadius: 24,
    borderWidth: 3,
    height: 240,
    marginTop: 80,
    width: 240,
  },
  detailsGrid: {
    gap: 12,
  },
  detailRow: {
    gap: 3,
  },
  detailLabel: {
    fontSize: 12,
    fontWeight: "700",
    textTransform: "uppercase",
  },
  detailValue: {
    fontSize: 16,
    lineHeight: 22,
  },
  statusBadge: {
    alignSelf: "flex-start",
    borderCurve: "continuous",
    borderRadius: 999,
    paddingHorizontal: 12,
    paddingVertical: 6,
  },
  statusText: {
    fontWeight: "700",
  },
  modal: {
    flex: 1,
  },
  modalHeader: {
    gap: 12,
    padding: 20,
    paddingTop: 64,
  },
  modalFooter: {
    padding: 16,
  },
});

export default assetStyles;
