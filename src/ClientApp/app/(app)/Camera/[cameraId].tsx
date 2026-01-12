import { ScrollView, Text, TouchableOpacity, View } from "react-native";

import CameraStream from "@/features/cameras/components/CameraStream/CameraStream";
import { PtzDirection } from "@/features/cameras/utils";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import cameraViewerStyles from "@/features/cameras/components/CameraViewer.styles";
import { useStartPtzMovement } from "@/features/cameras/api/start-ptz-movement";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetCameraFromCacheOrApi from "@/features/cameras/hooks/useGetCameraFromCacheOrApi";
import { useLocalSearchParams } from "expo-router";

const joystickDirections: { label: string; key: PtzDirection }[] = [
  { label: "↑", key: "Up" },
  { label: "↓", key: "Down" },
  { label: "←", key: "Left" },
  { label: "→", key: "Right" },
];

const CameraScreen = () => {
  const { cameraId, siteId } = useLocalSearchParams<{
    cameraId: string;
    siteId: string;
  }>();
  const colorPalette = useColorPalette();

  const camera = useGetCameraFromCacheOrApi(siteId, cameraId);
  const { mutate: startPtzMovement } = useStartPtzMovement();

  const handleDirection = (direction: PtzDirection) => {
    startPtzMovement({
      ipAddress: camera.ipAddress,
      username: camera.username,
      password: camera.password,
      direction,
    });
  };

  return (
    // <SafeAreaView
    //   style={[
    //     cameraViewerStyles.container,
    //     { backgroundColor: colorPalette.background },
    //   ]}
    // >
    <ScrollView>
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
    </ScrollView>
    // </SafeAreaView>
  );
};

export default CameraScreen;
