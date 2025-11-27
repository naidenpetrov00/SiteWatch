import { Image, Pressable, Text, View } from "react-native";

import { Camera } from "../../api/types";
import React from "react";
import { cameraCardStyles } from "./CameraCard.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

type Props = {
  camera: Camera;
  onPress?: () => void;
};

const CameraCard: React.FC<Props> = ({ camera, onPress }) => {
  const colorPalette = useColorPalette();

  return (
    <Pressable
      onPress={onPress}
      style={[cameraCardStyles.card, { backgroundColor: colorPalette.background }]}
    >
      <View style={cameraCardStyles.header}>
        <Text style={[cameraCardStyles.title, { color: colorPalette.text }]}>
          {camera.name || `Camera ${camera.id}`}
        </Text>
      </View>

      <View style={cameraCardStyles.snapshotWrapper}>
        <Image style={cameraCardStyles.snapshot} resizeMode="cover" />
      </View>
    </Pressable>
  );
};

export default CameraCard;
