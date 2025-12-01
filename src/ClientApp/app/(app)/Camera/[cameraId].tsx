import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { Text, TouchableOpacity, View } from "react-native";
import { useLocalSearchParams } from "expo-router";

import { useColorPalette } from "@/hooks/useColorPalette";
import cameraViewerStyles from "@/features/cameras/components/CameraViewer.styles";

const joystickDirections = [
  { label: "↑", key: "up" },
  { label: "↓", key: "down" },
  { label: "←", key: "left" },
  { label: "→", key: "right" },
];

const CameraScreen = () => {
  const { cameraId } = useLocalSearchParams<{ cameraId: string }>();
  const colorPalette = useColorPalette();

  const handleDirection = (direction: string) => {
    console.log(`Move camera ${cameraId} ${direction}`);
  };

  return (
    <SafeAreaView
      style={[
        cameraViewerStyles.container,
        { backgroundColor: colorPalette.background },
      ]}
    >
      <View
        style={[
          cameraViewerStyles.streamWrapper,
          {
            borderColor: colorPalette.primary,
          },
        ]}
      >
        <Text style={[cameraViewerStyles.streamLabel, { color: colorPalette.text }]}>
          Camera Stream
        </Text>
      </View>

      <View style={cameraViewerStyles.detailsWrapper}>
        <Text style={[cameraViewerStyles.cameraName, { color: colorPalette.text }]}>
          Camera {cameraId}
        </Text>
        <Text style={{ color: colorPalette.secondary }}>
          Live view with manual controls
        </Text>
      </View>

      <View style={cameraViewerStyles.joystickWrapper}>
        <Text style={[cameraViewerStyles.joystickLabel, { color: colorPalette.text }]}>
          Movement Joystick
        </Text>
        <View
          style={[
            cameraViewerStyles.joystick,
            {
              borderColor: colorPalette.primary,
              backgroundColor: colorPalette.background,
            },
          ]}
        >
          {joystickDirections.map((direction) => (
            <TouchableOpacity
              key={direction.key}
              style={[
                cameraViewerStyles.directionButton,
                { backgroundColor: colorPalette.primary },
              ]}
              onPress={() => handleDirection(direction.key)}
            >
              <Text style={{ color: colorPalette.background, fontSize: 18 }}>
                {direction.label}
              </Text>
            </TouchableOpacity>
          ))}
        </View>
      </View>
    </SafeAreaView>
  );
};

export default CameraScreen;
