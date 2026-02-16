import { StyleSheet } from "react-native";

const authPageTitleStyles = StyleSheet.create({
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
  },
  subtitleText: {
    fontSize: 13,
    textAlign: "center",
  },
  subtitleLinkText: {
    textDecorationLine: "underline",
    fontWeight: "600",
  },
});

export default authPageTitleStyles;
