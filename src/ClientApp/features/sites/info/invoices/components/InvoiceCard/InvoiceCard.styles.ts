import { StyleSheet } from "react-native";

const invoiceCardStyles = StyleSheet.create({
  card: {
    flexDirection: "row",
    alignItems: "stretch",
    borderWidth: 1,
    borderRadius: 16,
    borderCurve: "continuous",
    overflow: "hidden",
  },
  body: {
    flex: 1,
    gap: 5,
    padding: 16,
  },
  pressed: {
    opacity: 0.72,
  },
  invoiceNumber: {
    fontSize: 17,
    fontWeight: "700",
  },
  supplier: {
    fontSize: 14,
    fontWeight: "600",
  },
  metadata: {
    fontSize: 13,
    lineHeight: 18,
  },
  fileButton: {
    width: 58,
    alignItems: "center",
    justifyContent: "center",
    borderLeftWidth: 1,
  },
});

export default invoiceCardStyles;
