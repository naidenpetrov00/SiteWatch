import { Modal, Pressable, StatusBar, View } from "react-native";
import { PlaybackMethods, VLCPlayer } from "react-native-vlc-media-player";
import React, { useState } from "react";

import OverlayControls from "./OverlayControls";
import { SafeAreaView } from "react-native-safe-area-context";
import { playerStyles } from "./Player.styles";
import useOverlayVisibility from "../../hooks/useOverlayVisibility";
import usePlayerOrientation from "../../hooks/usePlayerOrientation";

interface PlayerProps {
  rtsp: string;
  joystick?: React.ReactNode;
}

const Player: React.FC<PlayerProps> = ({ rtsp, joystick }) => {
  const [isMuted, setIsMuted] = useState(true);
  const { isLandscape, toggleFullscreen } = usePlayerOrientation();
  const { overlayVisible, handleOverlayPress, onInteraction } =
    useOverlayVisibility(isLandscape);
    
  const playerContent = (
    <Pressable
      style={[
        playerStyles.streamWrapper,
        isLandscape && playerStyles.fullscreenWrapper,
      ]}
      onPress={handleOverlayPress}
    >
      <VLCPlayer
    
        style={[
          playerStyles.video,
          isLandscape && playerStyles.fullscreenVideo,
        ]}
        autoAspectRatio={true}
        muted={isMuted}
        volume={isMuted ? 0 : 100}
        source={{
          uri: rtsp,
          initOptions: ["--rtsp-tcp"],
        }}
        resizeMode="fill"
      />
      <OverlayControls
        visible={overlayVisible}
        isMuted={isMuted}
        isLandscape={isLandscape}
        onInteraction={onInteraction}
        setIsMuted={setIsMuted}
        onToggleFullscreen={toggleFullscreen}
      />
      {isLandscape && joystick ? (
        <View style={playerStyles.joystickOverlay}>{joystick}</View>
      ) : null}
    </Pressable>
  );

  const content = isLandscape ? (
    <SafeAreaView
      style={playerStyles.fullscreenContainer}
      edges={["right", "left"]}
    >
      {playerContent}
    </SafeAreaView>
  ) : (
    playerContent
  );

  return isLandscape ? (
    <Modal visible animationType="fade" presentationStyle="fullScreen">
      <StatusBar hidden />
      {content}
    </Modal>
  ) : (
    content
  );
};

export default Player;
