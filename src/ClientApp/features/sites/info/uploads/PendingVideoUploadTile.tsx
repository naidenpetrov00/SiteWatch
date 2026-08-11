import { ActivityIndicator, Text, View } from "react-native";
import Animated, { FadeInDown } from "react-native-reanimated";

import { useColorPalette } from "@/hooks/useColorPalette";
import { siteVideosStyles } from "../videos/component/Videos/Videos.styles";

type PendingVideoUploadTileProps = {
  tileWidth: number;
};

const PendingVideoUploadTile = ({ tileWidth }: PendingVideoUploadTileProps) => {
  const colorPalette = useColorPalette();

  return (
    <Animated.View
      entering={FadeInDown.duration(180)}
      accessibilityLabel="Video uploading"
      style={[
        siteVideosStyles.galleryTile,
        {
          alignItems: "center",
          backgroundColor: `${colorPalette.secondary}33`,
          justifyContent: "center",
          width: tileWidth,
        },
      ]}
    >
      <ActivityIndicator color={colorPalette.primary} />
      <Text style={{ color: colorPalette.text, fontWeight: "600" }}>
        Uploading video…
      </Text>
    </Animated.View>
  );
};

export default PendingVideoUploadTile;
