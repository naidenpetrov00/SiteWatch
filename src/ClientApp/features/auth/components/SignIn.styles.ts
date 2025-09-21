import { StyleSheet } from "react-native";
const RADIUS = 14;

const signInStyles = StyleSheet.create({
  safe: {
    flex: 1,
  },
  container: {
    flex: 1,
    paddingHorizontal: 24,
  },
  logoBadge: {
    width: 62,
    height: 62,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
    marginTop: 8,
  },
  logoText: {
    fontWeight: "700",
  },
  headerWrap: {
    marginTop: 24,
    alignItems: "center",
  },
  title: {
    fontSize: 32,
    lineHeight: 38,
    fontWeight: "800",
    textAlign: "center",
  },
  subtitle: {
    marginTop: 6,
    fontSize: 13,
    textAlign: "center",
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

export default signInStyles;
