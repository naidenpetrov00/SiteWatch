import { ActivityIndicator, Image, Pressable, Text, View } from "react-native";

import { Camera } from "../../api/models";
import Card from "@/components/ui/Card/Card";
import React from "react";
import { cameraCardStyles } from "./CameraCard.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import {
  useGetCameraSnapshot,
} from "../../api/get-camera-snapshot";

type Props = {
  camera: Camera;
  onPress?: () => void;
  onManage?: () => void;
};

const CameraCard: React.FC<Props> = ({ camera, onPress, onManage }) => {
  const colorPalette = useColorPalette();

  const {
    data: snapshotUri,
    isLoading,
    error,
  } = useGetCameraSnapshot({
    cameraId: camera.id,
  });

  return (
    <View style={cameraCardStyles.cardContainer}>
      <Card
        onPress={onPress}
        backgroundColor={colorPalette.background}
        borderColor={colorPalette.primary}
        style={cameraCardStyles.card}
      >
        <View style={cameraCardStyles.header}>
          <Text style={[cameraCardStyles.title, { color: colorPalette.text }]}>
            {camera.name || `Camera ${camera.id}`}
          </Text>
        </View>

        <View style={cameraCardStyles.snapshotWrapper}>
          {isLoading ? (
            <ActivityIndicator color={colorPalette.primary} size={"large"} />
          ) : error ? (
            <Text>No Snapshot</Text>
          ) : snapshotUri ? (
            <Image
              style={cameraCardStyles.snapshot}
              resizeMode="cover"
              source={{ uri: snapshotUri }}
            />
          ) : (
            <Text>No snapshot</Text>
          )}
        </View>
      </Card>
      {onManage ? (
        <Pressable
          accessibilityLabel={`Manage ${camera.name || "camera"}`}
          accessibilityRole="button"
          onPress={onManage}
          style={({ pressed }) => [
            cameraCardStyles.manageButton,
            {
              backgroundColor: colorPalette.primary,
              opacity: pressed ? 0.78 : 1,
            },
          ]}
        >
          <Text style={{ color: colorPalette.background, fontWeight: "600" }}>
            Manage
          </Text>
        </Pressable>
      ) : null}
    </View>
  );
};

export default CameraCard;
