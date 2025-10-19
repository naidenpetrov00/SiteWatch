import { StyleSheet } from "react-native";
const RADIUS = 14;

const signUpFormStyles = StyleSheet.create({
  form: {
    marginTop: 28,
  },
  cta: {
    marginTop: 22,
    borderRadius: RADIUS,
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 14,
  },
  ctaText: {
    fontWeight: "700",
    fontSize: 16,
  },
});

export default signUpFormStyles;
