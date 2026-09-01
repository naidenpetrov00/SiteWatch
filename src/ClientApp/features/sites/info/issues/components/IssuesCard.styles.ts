import { StyleSheet } from "react-native";

const issuesCardStyles = StyleSheet.create({
  container: {
    flex: 1,
    gap: 14,
    paddingHorizontal: 16,
    paddingTop: 16,
  },
  header: {
    alignItems: "center",
    flexDirection: "row",
    gap: 12,
    justifyContent: "space-between",
  },
  heading: {
    flex: 1,
    gap: 3,
  },
  title: {
    fontSize: 20,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 14,
  },
  addButton: {
    alignItems: "center",
    borderRadius: 10,
    flexDirection: "row",
    gap: 6,
    paddingHorizontal: 12,
    paddingVertical: 9,
  },
  addButtonText: {
    fontSize: 14,
    fontWeight: "700",
  },
  list: {
    gap: 10,
    paddingBottom: 24,
  },
  row: {
    borderRadius: 12,
    borderWidth: 1,
    gap: 4,
    padding: 13,
  },
  rowHeader: {
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
    justifyContent: "space-between",
  },
  issueTitle: {
    flex: 1,
    fontSize: 16,
    fontWeight: "700",
  },
  reference: {
    fontSize: 13,
    fontWeight: "600",
  },
  status: {
    borderRadius: 999,
    fontSize: 12,
    fontWeight: "700",
    overflow: "hidden",
    paddingHorizontal: 8,
    paddingVertical: 4,
  },
  state: {
    alignItems: "center",
    borderRadius: 12,
    borderWidth: 1,
    gap: 5,
    padding: 20,
  },
  stateTitle: {
    fontSize: 15,
    fontWeight: "700",
  },
  stateText: {
    fontSize: 14,
    textAlign: "center",
  },
});

export default issuesCardStyles;
