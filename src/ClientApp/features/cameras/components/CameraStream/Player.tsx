import * as FileSystem from "expo-file-system/legacy";
import * as MediaLibrary from "expo-media-library";

import { Alert, Modal, Pressable, StatusBar, View } from "react-native";
import { PlaybackMethods, VLCPlayer } from "react-native-vlc-media-player";
import React, {
  useCallback,
  useImperativeHandle,
  useRef,
  useState,
} from "react";

import OverlayControls from "./OverlayControls";
import { PlayerHandle } from "../../types";
import { SafeAreaView } from "react-native-safe-area-context";
import { playerStyles } from "./Player.styles";
import useOverlayVisibility from "../../hooks/useOverlayVisibility";
import usePlayerOrientation from "../../hooks/usePlayerOrientation";

const recordsDir = FileSystem.documentDirectory + "records/";
const recordsDirFs = recordsDir.replace(/^file:\/\//, "");

const ensureDir = async () => {
  const info = await FileSystem.getInfoAsync(recordsDir);
  if (!info.exists) {
    await FileSystem.makeDirectoryAsync(recordsDir, { intermediates: true });
  }
};

interface PlayerProps {
  rtsp: string;
  joystick?: React.ReactNode;
  isRecording: boolean;
  onRecordingChange?: (nextIsRecording: boolean) => void;
}

const Player = React.forwardRef<PlayerHandle, PlayerProps>(
  ({ rtsp, joystick, isRecording, onRecordingChange }, ref) => {
    const playerRef = useRef<VLCPlayer>(null);
    const [isMuted, setIsMuted] = useState(true);
    const { isLandscape, toggleFullscreen } = usePlayerOrientation();
    const { overlayVisible, handleOverlayPress, onInteraction } =
      useOverlayVisibility(isLandscape);

    const handleOnRecordingCreatedAsync = async (recordingPath: string) => {
      console.log("Recording created at:", recordingPath);

      const normalizedPath = recordingPath.startsWith("file://")
        ? recordingPath
        : `file://${recordingPath}`;

      const info = await FileSystem.getInfoAsync(normalizedPath);
      if (!info.exists) {
        console.warn("Recording file not found at:", normalizedPath);
        return;
      }

      try {
        const asset = await MediaLibrary.createAssetAsync(normalizedPath);
        await MediaLibrary.createAlbumAsync("MyApp Videos", asset, false);
        Alert.alert("Saved", "Snapshot saved to your device.");
      } catch (error) {
        console.warn("Failed to save recording:", error);
      }
    };

    const handleToggleRecording = useCallback(async () => {
      if (!playerRef.current) return;

      if (!isRecording) {
        await ensureDir();
        console.log("Starting recording to:", recordsDirFs);
        playerRef.current.startRecording(recordsDirFs);
        onRecordingChange?.(true);
      } else {
        console.log("Stopping recording.");
        playerRef.current.stopRecording();
        onRecordingChange?.(false);
      }
    }, [isRecording, onRecordingChange]);

    useImperativeHandle(
      ref,
      () => ({
        toggleRecording: handleToggleRecording,
      }),
      [handleToggleRecording],
    );

    const playerContent = (
      <Pressable
        style={[
          playerStyles.streamWrapper,
          isLandscape && playerStyles.fullscreenWrapper,
        ]}
        onPress={handleOverlayPress}
      >
        <VLCPlayer
          ref={playerRef}
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
          onRecordingCreated={(path) => {
            console.log("onRecordingCreated callback:", path);
            if (path) handleOnRecordingCreatedAsync(path);
          }}
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
  },
);

export default Player;
