import { Text, View } from "react-native";

import React from "react";
import { cameraStreamStyles } from "./CameraStream.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface CameraStreamProps {
  label?: string;
}

const CameraStream: React.FC<CameraStreamProps> = ({
  label = "Camera Stream",
}) => {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[
        cameraStreamStyles.streamWrapper,
        { borderColor: colorPalette.primary },
      ]}
    >
      <Text
        style={[cameraStreamStyles.streamLabel, { color: colorPalette.text }]}
      >
        {label}
      </Text>
    </View>
  );
};

export default CameraStream;
