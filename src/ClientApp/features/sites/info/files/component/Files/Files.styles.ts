import { StyleSheet } from "react-native";

const filesStyles = StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: 16,
  },
  title: {
    fontSize: 24,
    fontWeight: "700",
  },
  subtitle: {
    marginTop: 4,
    marginBottom: 20,
    fontSize: 14,
  },
  filterScroller: {
    flexGrow: 0,
    flexShrink: 0,
    height: 46,
  },
  filters: {
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
    paddingRight: 16,
  },
  filterGroup: {
    gap: 8,
    marginBottom: 18,
  },
  filterLabel: {
    fontSize: 14,
    fontWeight: "700",
  },
  filterChip: {
    borderWidth: 1,
    borderRadius: 999,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  filterText: {
    fontSize: 14,
    fontWeight: "600",
  },
  listContent: {
    gap: 12,
    paddingBottom: 24,
  },
  fileRow: {
    borderWidth: 1,
    borderRadius: 12,
    padding: 16,
  },
  fileName: {
    fontSize: 16,
    fontWeight: "600",
  },
  metadata: {
    marginTop: 4,
    fontSize: 13,
  },
  emptyState: {
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    borderStyle: "dashed",
    borderRadius: 16,
    padding: 24,
  },
  description: {
    marginTop: 8,
    fontSize: 14,
    textAlign: "center",
  },
});

export default filesStyles;
