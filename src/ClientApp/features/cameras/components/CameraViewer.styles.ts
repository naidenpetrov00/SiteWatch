import { Dimensions, StyleSheet } from "react-native";

const { width } = Dimensions.get("window");
const streamHeight = width * 0.62;

const cameraViewerStyles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 20,
    gap: 24,
  },
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
  detailsWrapper: {
    gap: 4,
  },
  cameraName: {
    fontSize: 20,
    fontWeight: "600",
  },
  joystickWrapper: {
    gap: 16,
    flex: 1,
  },
  joystickLabel: {
    fontSize: 18,
    fontWeight: "500",
  },
  joystick: {
    flex: 1,
    borderWidth: 2,
    borderRadius: 150,
    alignItems: "center",
    justifyContent: "space-around",
    paddingVertical: 24,
  },
  directionButton: {
    width: 64,
    height: 64,
    borderRadius: 32,
    alignItems: "center",
    justifyContent: "center",
  },
});

export default cameraViewerStyles;
