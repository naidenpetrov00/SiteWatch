import { Image, Text, View } from "react-native";

import { Camera } from "../../api/types";
import Card from "@/components/ui/Card/Card";
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
        <Image style={cameraCardStyles.snapshot} resizeMode="cover" />
      </View>
    </Card>
  );
};

export default CameraCard;
