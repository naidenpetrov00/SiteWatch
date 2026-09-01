import { StyleSheet } from "react-native";

const issueDetailsModalStyles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    alignItems: "center",
    borderBottomWidth: 1,
    flexDirection: "row",
    gap: 14,
    justifyContent: "space-between",
    padding: 20,
  },
  heading: {
    flex: 1,
    gap: 3,
  },
  eyebrow: {
    fontSize: 13,
    fontWeight: "700",
    textTransform: "uppercase",
  },
  title: {
    fontSize: 22,
    fontWeight: "700",
  },
  closeButton: {
    alignItems: "center",
    borderRadius: 20,
    height: 40,
    justifyContent: "center",
    width: 40,
  },
  content: {
    gap: 20,
    padding: 20,
  },
  section: {
    gap: 8,
  },
  sectionTitle: {
    fontSize: 17,
    fontWeight: "700",
  },
  detailRow: {
    gap: 4,
    borderBottomWidth: 1,
    paddingVertical: 10,
  },
  label: {
    fontSize: 13,
    fontWeight: "600",
  },
  value: {
    fontSize: 15,
    lineHeight: 21,
  },
  state: {
    alignItems: "center",
    flex: 1,
    gap: 10,
    justifyContent: "center",
    padding: 24,
  },
  stateText: {
    fontSize: 15,
    textAlign: "center",
  },
});

export default issueDetailsModalStyles;
