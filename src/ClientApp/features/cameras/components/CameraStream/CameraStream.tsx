import React, { useRef } from "react";
import { Text, View } from "react-native";
import { VLCPlayer, VlCPlayerView } from "react-native-vlc-media-player";

import { Camera } from "../../api/models";
import { ChannelType } from "../../types";
import { cameraStreamStyles } from "./CameraStream.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetRtspUrl from "../../hooks/useGetRtspUrl";

interface CameraStreamProps {
  camera: Camera;
}

const CameraStream: React.FC<CameraStreamProps> = ({ camera }) => {
  const colorPalette = useColorPalette();
  const rtsp = useGetRtspUrl(camera, ChannelType.Main);

  const renderCount = React.useRef(0);
  renderCount.current++;

  return (
    <View
      style={[
        cameraStreamStyles.streamWrapper,
        { borderColor: colorPalette.primary },
      ]}
    >
      <VLCPlayer
        style={cameraStreamStyles.video}
        autoAspectRatio={true}
        source={{
          uri: rtsp,
        }}
        resizeMode="fill"
      />
      {/*<VlCPlayerView*/}
      {/*    autoplay={true}*/}
      {/*    url="https://www.radiantmediaplayer.com/media/big-buck-bunny-360p.mp4"*/}
      {/*    ggUrl=""*/}
      {/*    showGG={true}*/}
      {/*    showTitle={true}*/}
      {/*    title="Big Buck Bunny"*/}
      {/*    showBack={true}*/}
      {/*    onLeftPress={() => {*/}
      {/*    }}*/}
      {/*/>*/}
      <Text
        style={[
          cameraStreamStyles.streamLabel,
          { color: colorPalette.text, zIndex: 10000 },
        ]}
      >
        {renderCount.current}
      </Text>
    </View>
  );
};

export default CameraStream;
