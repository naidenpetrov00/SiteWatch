import { StyleSheet } from "react-native";

export const playerStyles = StyleSheet.create({
  streamWrapper: {
    position: "relative",
    overflow: "hidden",
  },
  fullscreenWrapper: {
    flex: 1,
  },
  video: {
    width: "100%",
    aspectRatio: 16 / 9,
  },
  fullscreenVideo: {
    width: "100%",
    height: "100%",
    aspectRatio: undefined,
  },
  fullscreenContainer: {
    flex: 1,
    backgroundColor: "black",
    justifyContent: "center",
  },
});
