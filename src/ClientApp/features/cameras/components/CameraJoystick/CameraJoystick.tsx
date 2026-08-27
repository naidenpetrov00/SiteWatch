import {
    StyleProp,
    Text,
    TextStyle,
    TouchableOpacity,
    View,
    ViewStyle,
} from "react-native";

import {Camera} from "@/features/cameras/api/models";
import {MaterialIcons} from "@expo/vector-icons";
import {PtzDirection} from "@/features/cameras/utils";
import React from "react";
import cameraJoystickStyles from "@/features/cameras/components/CameraJoystick/CameraJoystick.styles";
import {useColorPalette} from "@/hooks/useColorPalette";
import {useMoveRelativePtz} from "@/features/cameras/api/move-relative-ptz";
import {useStartPtzMovement} from "@/features/cameras/api/start-ptz-movement";
import {useStopPtzMovement} from "@/features/cameras/api/stop-ptz-movement";
import {useWindowDimensions} from "react-native";
import {
    createPtzMetricContext,
    logPtzMetric,
    type PtzMetricContext,
} from "@/features/cameras/latency-metrics";

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
    const {width, height} = useWindowDimensions();
    const isLandscape = width > height;
    const buttonSize = isLandscape ? 48 : 64;
    const iconSize = isLandscape ? 20 : 24;
    const {mutate: startPtzMovement} = useStartPtzMovement();
    const {mutate: stopPtzMovement} = useStopPtzMovement();
    const {mutate: moveRelativePtz} = useMoveRelativePtz();
    const pressTimeoutRef = React.useRef<ReturnType<typeof setTimeout> | null>(
        null
    );
    const isLongPressRef = React.useRef(false);
    const interactionRef = React.useRef<PtzMetricContext | null>(null);

    const handleStart = (direction: PtzDirection, metric: PtzMetricContext) => {
        startPtzMovement({
            cameraId: camera.id,
            direction,
            metric,
        });
    };

    const handleStop = (direction: PtzDirection, metric: PtzMetricContext) => {
        stopPtzMovement({
            cameraId: camera.id,
            direction,
            metric,
        });
    };

    const handleRelativeMove = (direction: PtzDirection, metric: PtzMetricContext) => {
        const motion =
            direction === "Left"
                ? {arg1: -relativeStep, arg2: 0}
                : direction === "Right"
                    ? {arg1: relativeStep, arg2: 0}
                    : direction === "Up"
                        ? {arg1: 0, arg2: relativeStep}
                        : {arg1: 0, arg2: -relativeStep};

        moveRelativePtz({
            cameraId: camera.id,
            horizontal: motion.arg1,
            vertical: motion.arg2,
            zoom: 0,
            metric,
        });
    };

    const handlePressIn = (direction: PtzDirection) => {
        if (pressTimeoutRef.current) {
            clearTimeout(pressTimeoutRef.current);
        }
        isLongPressRef.current = false;
        interactionRef.current = createPtzMetricContext(camera.id, direction);
        logPtzMetric(interactionRef.current, "interaction");
        pressTimeoutRef.current = setTimeout(() => {
            isLongPressRef.current = true;
            if (interactionRef.current) {
                handleStart(direction, interactionRef.current);
            }
        }, longPressDelayMs);
    };

    const handlePressOut = (direction: PtzDirection) => {
        if (pressTimeoutRef.current) {
            clearTimeout(pressTimeoutRef.current);
            pressTimeoutRef.current = null;
        }

        if (isLongPressRef.current) {
            if (interactionRef.current) {
                handleStop(direction, interactionRef.current);
            }
            isLongPressRef.current = false;
            return;
        }

        if (interactionRef.current) {
            handleRelativeMove(direction, interactionRef.current);
        }
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
