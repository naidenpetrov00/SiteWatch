import { StyleSheet } from "react-native";
const RADIUS = 14;

const logoStyles = StyleSheet.create({
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
});

export default logoStyles;
