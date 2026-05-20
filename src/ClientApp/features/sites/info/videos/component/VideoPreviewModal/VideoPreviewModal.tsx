import {
  ActivityIndicator,
  Image,
  Modal,
  StatusBar,
  Text,
  View,
} from "react-native";
import { VideoView, useVideoPlayer } from "expo-video";
import { useEffect, useState } from "react";

import { SafeAreaView } from "react-native-safe-area-context";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetSiteVideoFull } from "../../hooks/useGetSiteVideoFull";
import { videoPreviewModalStyles } from "./VideoPreviewModal.styles";
import type { VisibleSiteVideo } from "../../types";
import VideoPreviewHeader from "./VideoPreviewHeader";

interface IVideoPreviewModal {
  video: VisibleSiteVideo | null;
  visible: boolean;
  onClose: () => void;
}

interface IVideoPreviewModalContent {
  video: VisibleSiteVideo;
  visible: boolean;
  onClose: () => void;
}

interface IVideoPreviewPlayer {
  sourceUri: string;
}

const VideoPreviewPlayer = ({ sourceUri }: IVideoPreviewPlayer) => {
  const [hasRenderedFirstFrame, setHasRenderedFirstFrame] = useState(false);
  const player = useVideoPlayer({ uri: sourceUri }, (videoPlayer) => {
    videoPlayer.play();
  });

  useEffect(() => {
    setHasRenderedFirstFrame(false);
  }, [sourceUri]);

  return (
    <VideoView
      contentFit="contain"
      nativeControls
      onFirstFrameRender={() => setHasRenderedFirstFrame(true)}
      player={player}
      style={[
        videoPreviewModalStyles.previewVideo,
        hasRenderedFirstFrame ? null : videoPreviewModalStyles.previewVideoHidden,
      ]}
    />
  );
};

const VideoPreviewModalContent = ({
  video,
  visible,
  onClose,
}: IVideoPreviewModalContent) => {
  const colorPalette = useColorPalette();
  const { error, isFetching, uri: videoUri } = useGetSiteVideoFull({
    videoId: visible ? video.videoId : undefined,
    visible,
  });

  return (
    <Modal
      animationType="fade"
      onRequestClose={onClose}
      presentationStyle="fullScreen"
      visible={visible}
    >
      <StatusBar hidden />
      <SafeAreaView style={videoPreviewModalStyles.container}>
        <VideoPreviewHeader onClose={onClose} />
        <View style={videoPreviewModalStyles.previewWrapper}>
          <Image
            source={{ uri: video.snapshotUri }}
            resizeMode="contain"
            style={videoPreviewModalStyles.previewImage}
          />
          {videoUri ? <VideoPreviewPlayer sourceUri={videoUri} /> : null}
          {isFetching ? (
            <View style={videoPreviewModalStyles.loadingOverlay}>
              <ActivityIndicator
                color={colorPalette.contrastText}
                size="large"
              />
            </View>
          ) : null}
          {error ? (
            <View style={videoPreviewModalStyles.errorOverlay}>
              <Text style={videoPreviewModalStyles.errorText}>
                {error}
              </Text>
            </View>
          ) : null}
        </View>
      </SafeAreaView>
    </Modal>
  );
};

const VideoPreviewModal = ({ video, visible, onClose }: IVideoPreviewModal) => {
  if (!video) {
    return null;
  }

  return (
    <VideoPreviewModalContent
      onClose={onClose}
      video={video}
      visible={visible}
    />
  );
};

export default VideoPreviewModal;
