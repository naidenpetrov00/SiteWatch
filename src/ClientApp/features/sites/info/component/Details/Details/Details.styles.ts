import { StyleSheet } from "react-native";

const detailsStyles = StyleSheet.create({
  container: {
    paddingHorizontal: 16,
    paddingTop: 16,
  },
  grid: {
    gap: 12,
  },
  row: {
    gap: 12,
  },
  card: {
    flex: 1,
  },
  label: {
    fontSize: 12,
    fontWeight: "600",
    letterSpacing: 0.5,
    textTransform: "uppercase",
  },
  value: {
    fontSize: 28,
    fontWeight: "700",
  },
  helper: {
    fontSize: 13,
    lineHeight: 18,
  },
});

export default detailsStyles;
