import { StyleSheet } from "react-native";

export const cameraCardStyles = StyleSheet.create({
  card: {
    borderRadius: 12,
    overflow: "hidden",
    marginBottom: 16,
    elevation: 2,
  },
  header: {
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  title: {
    fontWeight: "600",
    fontSize: 14,
  },
  snapshotWrapper: {
    width: "100%",
    aspectRatio: 16 / 9, // "window" feel
    backgroundColor: "#111",
  },
  snapshot: {
    width: "100%",
    height: "100%",
  },
});
