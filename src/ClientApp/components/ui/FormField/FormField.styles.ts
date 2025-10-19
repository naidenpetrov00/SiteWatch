import { StyleSheet } from "react-native";

const formFieldStyles = StyleSheet.create({
  label: {
    marginTop: 16,
    fontSize: 12,
    letterSpacing: 1,
    marginBottom: 8,
  },
  input: {
    paddingHorizontal: 16,
    paddingVertical: 14,
    borderRadius: 14,
    fontSize: 16,
  },
  errorText: { marginTop: 6, color: "#ff6b6b", fontSize: 12 },
});

export default formFieldStyles;
