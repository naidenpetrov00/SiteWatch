import { Text, View } from "react-native";
// import { VLCPlayer, VlCPlayerView } from "react-native-vlc-media-player";

import React from "react";
import { cameraStreamStyles } from "./CameraStream.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface CameraStreamProps {
  label?: string;
}

const videoSource =
  "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

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
      {/*<VLCPlayer*/}
      {/*  // style={[styles.video]}*/}
      {/*  videoAspectRatio="16:9"*/}
      {/*  source={{ uri: videoSource }}*/}
      {/*/>*/}
      <Text
        style={[cameraStreamStyles.streamLabel, { color: colorPalette.text }]}
      >
        {label}
      </Text>
    </View>
  );
};

export default CameraStream;
