import { ActivityIndicator, Image, Text, View } from "react-native";

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
};

const CameraCard: React.FC<Props> = ({ camera, onPress }) => {
  const colorPalette = useColorPalette();

  const {
    data: snapshotUri,
    isLoading,
    error,
  } = useGetCameraSnapshot({
    data: {
      ipAddress: camera.ipAddress,
      username: camera.username,
      password: camera.password,
    },
  });

  return (
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
  );
};

export default CameraCard;
