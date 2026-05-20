import { Image, Pressable, Text, View } from "react-native";

import type { VisibleSiteVideo } from "../../types";
import { siteVideosStyles } from "../Videos/Videos.styles";

interface IVideoItem {
  tileWidth: number;
  item: VisibleSiteVideo;
  onPress: (item: VisibleSiteVideo) => void;
}

const formatDuration = (durationSeconds?: number | null) => {
  if (durationSeconds == null || Number.isNaN(durationSeconds)) {
    return "0:00";
  }

  const totalSeconds = Math.max(0, Math.round(durationSeconds));
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  const secondsLabel = seconds.toString().padStart(2, "0");

  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, "0")}:${secondsLabel}`;
  }

  return `${minutes}:${secondsLabel}`;
};

const VideoItem = ({ tileWidth, item, onPress }: IVideoItem) => {
  const durationLabel = formatDuration(item.durationSeconds);

  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => onPress(item)}
      style={({ pressed }) => [
        siteVideosStyles.galleryTile,
        { width: tileWidth },
        pressed ? siteVideosStyles.galleryTilePressed : null,
      ]}
    >
      <Image
        source={{ uri: item.snapshotUri }}
        resizeMode="cover"
        style={siteVideosStyles.galleryImage}
      />
      <View style={siteVideosStyles.durationOverlay} pointerEvents="none">
        <View style={siteVideosStyles.durationBadge}>
          <Text style={siteVideosStyles.durationText}>{durationLabel}</Text>
        </View>
      </View>
    </Pressable>
  );
};

export default VideoItem;
