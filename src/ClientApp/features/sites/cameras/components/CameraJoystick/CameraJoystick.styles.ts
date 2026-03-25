import { StyleSheet } from "react-native";

const cameraJoystickStyles = StyleSheet.create({
  joystickWrapper: {
    gap: 16,
    flex: 1,
  },
  joystickLabel: {
    fontSize: 18,
    fontWeight: "500",
  },
  joystick: {
    borderWidth: 2,
    borderRadius: 999,
    alignItems: "center",
    justifyContent: "center",
    position: "relative",
    width: 240,
    height: 240,
    alignSelf: "center",
  },
  directionButton: {
    width: 64,
    height: 64,
    borderRadius: 32,
    alignItems: "center",
    justifyContent: "center",
  },
  directionUp: {
    position: "absolute",
    top: 16,
  },
  directionDown: {
    position: "absolute",
    bottom: 16,
  },
  directionLeft: {
    position: "absolute",
    left: 16,
  },
  directionRight: {
    position: "absolute",
    right: 16,
  },
});

export default cameraJoystickStyles;
