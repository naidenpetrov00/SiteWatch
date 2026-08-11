import { StyleSheet } from "react-native";

const styles = StyleSheet.create({
  container: {
    alignItems: "stretch",
    gap: 8,
  },
  panel: {
    gap: 10,
    borderRadius: 16,
    borderCurve: "continuous",
    borderWidth: 1,
    padding: 12,
  },
  panelTitle: {
    fontSize: 14,
    fontWeight: "700",
  },
  options: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 8,
  },
  option: {
    borderWidth: 1,
    borderRadius: 999,
    borderCurve: "continuous",
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  optionText: {
    fontSize: 13,
    fontWeight: "600",
  },
  sourceButton: {
    minHeight: 46,
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
    borderWidth: 1,
    borderRadius: 12,
    borderCurve: "continuous",
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  sourceText: {
    fontSize: 14,
    fontWeight: "600",
  },
  actionButton: {
    minHeight: 52,
    alignItems: "center",
    justifyContent: "center",
    borderRadius: 14,
    borderCurve: "continuous",
    paddingHorizontal: 18,
    paddingVertical: 12,
  },
  actionButtonText: {
    fontSize: 15,
    fontWeight: "700",
  },
  pressed: {
    opacity: 0.72,
  },
});

export default styles;
