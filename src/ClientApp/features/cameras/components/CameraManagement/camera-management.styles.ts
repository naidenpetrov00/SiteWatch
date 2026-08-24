import { StyleSheet } from "react-native";

const cameraManagementStyles = StyleSheet.create({
  content: {
    gap: 14,
    padding: 20,
  },
  footer: {
    flexDirection: "row",
    gap: 12,
    paddingHorizontal: 20,
    paddingBottom: 20,
  },
  field: {
    gap: 6,
  },
  input: {
    borderRadius: 10,
    borderWidth: 1,
    fontSize: 16,
    paddingHorizontal: 12,
    paddingVertical: 11,
  },
  button: {
    alignItems: "center",
    borderRadius: 10,
    flex: 1,
    paddingVertical: 13,
  },
  siteItem: {
    borderRadius: 10,
    borderWidth: 1,
    gap: 4,
    padding: 14,
  },
  error: {
    fontSize: 13,
  },
});

export default cameraManagementStyles;
