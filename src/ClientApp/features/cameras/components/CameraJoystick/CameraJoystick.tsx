import {
  StyleProp,
  Text,
  TextStyle,
  TouchableOpacity,
  View,
  ViewStyle,
} from "react-native";

import { Camera } from "@/features/cameras/api/models";
import { MaterialIcons } from "@expo/vector-icons";
import { PtzDirection } from "@/features/cameras/utils";
import React from "react";
import cameraJoystickStyles from "@/features/cameras/components/CameraJoystick/CameraJoystick.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useMoveRelativePtz } from "@/features/cameras/api/move-relative-ptz";
import { useStartPtzMovement } from "@/features/cameras/api/start-ptz-movement";
import { useStopPtzMovement } from "@/features/cameras/api/stop-ptz-movement";
import { useWindowDimensions } from "react-native";

const joystickDirections: PtzDirection[] = ["Up", "Down", "Left", "Right"];

const longPressDelayMs = 250;
const relativeStep = 0.1;

type CameraJoystickProps = {
  camera: Camera;
  showLabel?: boolean;
  wrapperStyle?: StyleProp<ViewStyle>;
  joystickStyle?: StyleProp<ViewStyle>;
  labelStyle?: StyleProp<TextStyle>;
};

const CameraJoystick = ({
  camera,
  wrapperStyle,
  joystickStyle,
}: CameraJoystickProps) => {
  const colorPalette = useColorPalette();
  const { width, height } = useWindowDimensions();
  const isLandscape = width > height;
  const buttonSize = isLandscape ? 48 : 64;
  const iconSize = isLandscape ? 20 : 24;
  const { mutate: startPtzMovement } = useStartPtzMovement();
  const { mutate: stopPtzMovement } = useStopPtzMovement();
  const { mutate: moveRelativePtz } = useMoveRelativePtz();
  const pressTimeoutRef = React.useRef<ReturnType<typeof setTimeout> | null>(
    null
  );
  const isLongPressRef = React.useRef(false);

  const handleStart = (direction: PtzDirection) => {
    startPtzMovement({
      ipAddress: camera.ipAddress,
      username: camera.username,
      password: camera.password,
      direction,
    });
  };

  const handleStop = (direction: PtzDirection) => {
    stopPtzMovement({
      ipAddress: camera.ipAddress,
      username: camera.username,
      password: camera.password,
      direction,
    });
  };

  const handleRelativeMove = (direction: PtzDirection) => {
    const motion =
      direction === "Left"
        ? { arg1: -relativeStep, arg2: 0 }
        : direction === "Right"
        ? { arg1: relativeStep, arg2: 0 }
        : direction === "Up"
        ? { arg1: 0, arg2: relativeStep }
        : { arg1: 0, arg2: -relativeStep };

    moveRelativePtz({
      ipAddress: camera.ipAddress,
      username: camera.username,
      password: camera.password,
      arg1: motion.arg1,
      arg2: motion.arg2,
      arg3: 0,
    });
  };

  const handlePressIn = (direction: PtzDirection) => {
    if (pressTimeoutRef.current) {
      clearTimeout(pressTimeoutRef.current);
    }
    isLongPressRef.current = false;
    pressTimeoutRef.current = setTimeout(() => {
      isLongPressRef.current = true;
      handleStart(direction);
    }, longPressDelayMs);
  };

  const handlePressOut = (direction: PtzDirection) => {
    if (pressTimeoutRef.current) {
      clearTimeout(pressTimeoutRef.current);
      pressTimeoutRef.current = null;
    }

    if (isLongPressRef.current) {
      handleStop(direction);
      isLongPressRef.current = false;
      return;
    }

    handleRelativeMove(direction);
  };

  const directionPositionStyles: Record<PtzDirection, StyleProp<ViewStyle>> = {
    Up: cameraJoystickStyles.directionUp,
    Down: cameraJoystickStyles.directionDown,
    Left: cameraJoystickStyles.directionLeft,
    Right: cameraJoystickStyles.directionRight,
  };
  const directionIconNames: Record<
    PtzDirection,
    "keyboard-arrow-up" | "keyboard-arrow-down" | "keyboard-arrow-left" | "keyboard-arrow-right"
  > = {
    Up: "keyboard-arrow-up",
    Down: "keyboard-arrow-down",
    Left: "keyboard-arrow-left",
    Right: "keyboard-arrow-right",
  };

  return (
    <View style={[cameraJoystickStyles.joystickWrapper, wrapperStyle]}>
      <View
        style={[
          cameraJoystickStyles.joystick,
          {
            borderColor: colorPalette.primary,
          },
          joystickStyle,
        ]}
      >
        {joystickDirections.map((direction) => (
          <TouchableOpacity
            key={direction}
            style={[
              cameraJoystickStyles.directionButton,
              directionPositionStyles[direction],
              {
                backgroundColor: colorPalette.primary,
                width: buttonSize,
                height: buttonSize,
                borderRadius: buttonSize / 2,
                opacity: isLandscape ? 0.5 : 1,
              },
            ]}
            onPressIn={() => handlePressIn(direction)}
            onPressOut={() => handlePressOut(direction)}
          >
            <MaterialIcons
              name={directionIconNames[direction]}
              size={iconSize}
              color={colorPalette.background}
            />
          </TouchableOpacity>
        ))}
      </View>
    </View>
  );
};

export default CameraJoystick;
