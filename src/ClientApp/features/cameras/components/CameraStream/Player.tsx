import * as FileSystem from "expo-file-system/legacy";
import * as MediaLibrary from "expo-media-library";

import { Modal, Pressable, StatusBar, View } from "react-native";
import { PlaybackMethods, VLCPlayer } from "react-native-vlc-media-player";
import React, {
  useCallback,
  useImperativeHandle,
  useRef,
  useState,
} from "react";

import OverlayControls from "./OverlayControls";
import { SafeAreaView } from "react-native-safe-area-context";
import { playerStyles } from "./Player.styles";
import useOverlayVisibility from "../../hooks/useOverlayVisibility";
import usePlayerOrientation from "../../hooks/usePlayerOrientation";

const recordsDir = FileSystem.documentDirectory + "records/";

async function ensureDir() {
  const info = await FileSystem.getInfoAsync(recordsDir);
  if (!info.exists) {
    await FileSystem.makeDirectoryAsync(recordsDir, { intermediates: true });
  }
}

interface PlayerProps {
  rtsp: string;
  joystick?: React.ReactNode;
  isRecording: boolean;
  onRecordingChange?: (nextIsRecording: boolean) => void;
}

export interface PlayerHandle {
  toggleRecording: () => Promise<void>;
}

const Player = React.forwardRef<PlayerHandle, PlayerProps>(
  ({ rtsp, joystick, isRecording, onRecordingChange }, ref) => {
    const playerRef = useRef<VLCPlayer>(null);
    const recordingPathRef = useRef<string | null>(null);
    const [isMuted, setIsMuted] = useState(true);
    const { isLandscape, toggleFullscreen } = usePlayerOrientation();
    const { overlayVisible, handleOverlayPress, onInteraction } =
      useOverlayVisibility(isLandscape);

    const handleOnRecordingCreatedAsync = async (recordingPath: string) => {
      console.log(`from handleOnRecordingCreatedAsync: ${recordingPath}`);

      const perm = await MediaLibrary.requestPermissionsAsync();
      if (perm.status !== "granted") return;

      const asset = await MediaLibrary.createAssetAsync(recordingPath);

      await MediaLibrary.createAlbumAsync("MyApp Videos", asset, false);
    };
    const handleToggleRecording = useCallback(async () => {
      if (!playerRef.current) return;

      if (!isRecording) {
        await ensureDir();
        playerRef.current.startRecording(recordsDir); // ✅ directory
        onRecordingChange?.(true);
      } else {
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
            console.log("from onRecordingCreated:", path);
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
