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
  filters: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 10,
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
  title: {
    fontSize: 18,
    fontWeight: "700",
    textAlign: "center",
  },
  description: {
    marginTop: 8,
    fontSize: 14,
    textAlign: "center",
  },
});

export default filesStyles;
