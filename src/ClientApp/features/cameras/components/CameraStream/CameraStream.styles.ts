import { Dimensions, StyleSheet } from "react-native";

const { width } = Dimensions.get("window");
const streamHeight = width * 0.62;

export const cameraStreamStyles = StyleSheet.create({
  streamWrapper: {
    width: "100%",
    height: streamHeight,
    borderWidth: 2,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(0,0,0,0.2)",
  },
  streamLabel: {
    fontSize: 18,
  },
});
