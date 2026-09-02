import Ionicons from "@expo/vector-icons/Ionicons";
import { VideoView, useVideoPlayer } from "expo-video";
import { Image, Modal, Pressable, StatusBar, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import type { IssueAttachmentKind } from "../types";
import styles from "./IssueDetailsPage.styles";

export type AttachmentViewerState = { kind: IssueAttachmentKind; name: string; url: string } | null;

const VideoViewer = ({ url }: { url: string }) => {
  const player = useVideoPlayer({ uri: url }, (videoPlayer) => videoPlayer.play());
  return <VideoView contentFit="contain" nativeControls player={player} style={styles.viewerVideo} />;
};

const IssueAttachmentViewer = ({ viewer, onClose }: { viewer: AttachmentViewerState; onClose: () => void }) => {
  if (!viewer) return null;

  return (
    <Modal animationType="fade" onRequestClose={onClose} presentationStyle="fullScreen" visible>
      <StatusBar hidden />
      <SafeAreaView style={styles.viewer}>
        <View style={styles.viewerHeader}><Text numberOfLines={1} style={styles.viewerTitle}>{viewer.name}</Text><Pressable accessibilityLabel="Close attachment viewer" accessibilityRole="button" hitSlop={8} onPress={onClose}><Ionicons color="#fff" name="close" size={28} /></Pressable></View>
        {viewer.kind === "Image" ? <Image resizeMode="contain" source={{ uri: viewer.url }} style={styles.viewerImage} /> : <VideoViewer url={viewer.url} />}
      </SafeAreaView>
    </Modal>
  );
};

export default IssueAttachmentViewer;
