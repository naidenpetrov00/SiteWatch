import { StyleSheet } from "react-native";

export const overlayControlsStyles = StyleSheet.create({
  overlay: {
    position: "absolute",
    left: 0,
    right: 0,
    bottom: 0,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingHorizontal: 16,
    paddingVertical: 8,
    backgroundColor: "rgba(0, 0, 0, 0.55)",
  },
  hidden: {
    opacity: 0,
  },
  controlButton: {
    padding: 10,
    backgroundColor: "rgba(255, 255, 255, 0.14)",
    borderRadius: 22,
  },
});
