import { Text, TouchableOpacity, View } from "react-native";

import CameraStream from "@/features/cameras/components/CameraStream/CameraStream";
import { ChannelType } from "@/features/cameras/types";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import cameraViewerStyles from "@/features/cameras/components/CameraViewer.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetCameraFromCacheOrApi from "@/features/cameras/hooks/useGetCameraFromCacheOrApi";
import useGetRtspUrl from "@/features/cameras/hooks/useGetRtspUrl";
import { useLocalSearchParams } from "expo-router";

const joystickDirections = [
  { label: "↑", key: "up" },
  { label: "↓", key: "down" },
  { label: "←", key: "left" },
  { label: "→", key: "right" },
];

const CameraScreen = () => {
  const { cameraId, siteId } = useLocalSearchParams<{
    cameraId: string;
    siteId: string;
  }>();
  const colorPalette = useColorPalette();

  const camera = useGetCameraFromCacheOrApi(siteId, cameraId);

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
      <CameraStream camera={camera} />
      <View style={cameraViewerStyles.content}>
        <View style={cameraViewerStyles.detailsWrapper}>
          <Text
            style={[
              cameraViewerStyles.cameraName,
              { color: colorPalette.text },
            ]}
          >
            Camera {cameraId}
          </Text>
          <Text style={{ color: colorPalette.secondary }}>
            Live view with manual controls
          </Text>
        </View>

        <View style={cameraViewerStyles.joystickWrapper}>
          <Text
            style={[
              cameraViewerStyles.joystickLabel,
              { color: colorPalette.text },
            ]}
          >
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
      </View>
    </SafeAreaView>
  );
};

export default CameraScreen;
