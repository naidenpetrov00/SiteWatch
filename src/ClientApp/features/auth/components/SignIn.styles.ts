import { StyleSheet } from "react-native";
const RADIUS = 14;

const signUpStyles = StyleSheet.create({
  safe: {
    flex: 1,
  },
  container: {
    flex: 1,
    paddingHorizontal: 24,
  },
 
 
  form: {
    marginTop: 28,
  },
  label: {
    marginTop: 16,
    fontSize: 12,
    letterSpacing: 1,
    marginBottom: 8,
  },
  input: {
    paddingHorizontal: 16,
    paddingVertical: 14,
    borderRadius: RADIUS,
    fontSize: 16,
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
  footer: {
    marginTop: 22,
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
  },
  footerMuted: {
    fontSize: 13,
  },
  footerLink: {
    fontSize: 13,
    fontWeight: "700",
  },
});

export default signUpStyles;
