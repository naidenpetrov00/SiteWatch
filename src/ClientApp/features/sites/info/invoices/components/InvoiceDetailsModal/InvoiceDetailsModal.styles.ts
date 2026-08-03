import { StyleSheet } from "react-native";

const invoiceDetailsModalStyles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    borderBottomWidth: 1,
    paddingHorizontal: 18,
    paddingVertical: 14,
  },
  heading: {
    flex: 1,
    gap: 3,
  },
  eyebrow: {
    fontSize: 12,
    fontWeight: "700",
    letterSpacing: 0.7,
    textTransform: "uppercase",
  },
  title: {
    fontSize: 20,
    fontWeight: "700",
  },
  closeButton: {
    alignItems: "center",
    justifyContent: "center",
    width: 44,
    height: 44,
    borderRadius: 22,
    borderCurve: "continuous",
  },
  pressed: {
    opacity: 0.65,
  },
  content: {
    gap: 22,
    padding: 20,
    paddingBottom: 36,
  },
  section: {
    gap: 10,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: "700",
  },
  detailGrid: {
    gap: 10,
  },
  detailRow: {
    gap: 3,
    borderBottomWidth: StyleSheet.hairlineWidth,
    paddingBottom: 10,
  },
  label: {
    fontSize: 12,
    fontWeight: "600",
    letterSpacing: 0.4,
    textTransform: "uppercase",
  },
  value: {
    fontSize: 15,
    lineHeight: 21,
  },
});

export default invoiceDetailsModalStyles;
