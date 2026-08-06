import { StyleSheet } from "react-native";

const invoicesStyles = StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: 16,
  },
  header: {
    gap: 4,
    paddingBottom: 18,
  },
  title: {
    fontSize: 24,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 14,
  },
  addButton: {
    alignSelf: "flex-start",
    borderRadius: 12,
    borderCurve: "continuous",
    paddingHorizontal: 16,
    paddingVertical: 11,
  },
  addButtonText: {
    fontSize: 14,
    fontWeight: "700",
  },
  sourceActions: {
    gap: 8,
    paddingTop: 4,
  },
  sourceButton: {
    minHeight: 46,
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
    borderWidth: 1,
    borderRadius: 12,
    borderCurve: "continuous",
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  sourceButtonText: {
    fontSize: 14,
    fontWeight: "600",
  },
  pressed: {
    opacity: 0.72,
  },
  success: {
    borderRadius: 12,
    borderCurve: "continuous",
    padding: 12,
    backgroundColor: "rgba(22, 163, 74, 0.12)",
    fontSize: 14,
  },
  error: {
    borderWidth: 1,
    borderRadius: 12,
    borderCurve: "continuous",
    padding: 12,
    fontSize: 14,
  },
  listContent: {
    gap: 12,
    paddingBottom: 24,
  },
  emptyState: {
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    borderWidth: 1,
    borderStyle: "dashed",
    borderRadius: 16,
    borderCurve: "continuous",
    padding: 28,
  },
  emptyTitle: {
    fontSize: 16,
    fontWeight: "700",
  },
  emptyDescription: {
    fontSize: 14,
    lineHeight: 20,
    textAlign: "center",
  },
});

export default invoicesStyles;
