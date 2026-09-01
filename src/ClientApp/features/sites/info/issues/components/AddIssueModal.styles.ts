import { StyleSheet } from "react-native";

const addIssueModalStyles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    gap: 16,
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: "700",
  },
  subtitle: {
    fontSize: 14,
    lineHeight: 20,
  },
  field: {
    gap: 7,
  },
  label: {
    fontSize: 14,
    fontWeight: "600",
  },
  input: {
    borderRadius: 10,
    borderWidth: 1,
    fontSize: 16,
    paddingHorizontal: 12,
    paddingVertical: 11,
  },
  descriptionInput: {
    minHeight: 150,
    textAlignVertical: "top",
  },
  error: {
    borderRadius: 10,
    borderWidth: 1,
    fontSize: 14,
    padding: 12,
  },
  footer: {
    flexDirection: "row",
    gap: 12,
    padding: 20,
  },
  button: {
    alignItems: "center",
    borderRadius: 10,
    flex: 1,
    paddingVertical: 13,
  },
  buttonText: {
    fontSize: 15,
    fontWeight: "700",
  },
});

export default addIssueModalStyles;
