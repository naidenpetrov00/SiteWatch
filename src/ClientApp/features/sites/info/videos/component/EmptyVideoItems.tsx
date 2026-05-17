import { Text, View } from "react-native";

import { siteVideosStyles } from "./SiteVideos.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const EmptyVideoItem = () => {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[
        siteVideosStyles.emptyState,
        { borderColor: colorPalette.secondary + "55" },
      ]}
    >
      <Text
        style={[siteVideosStyles.emptyText, { color: colorPalette.secondary }]}
      >
        No videos in this filter yet.
      </Text>
    </View>
  );
};

export default EmptyVideoItem;