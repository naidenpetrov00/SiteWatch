import { ActivityIndicator, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import { siteVideosStyles } from "../videos/component/Videos/Videos.styles";

type PendingVideoUploadTileProps = {
  tileWidth: number;
};

const PendingVideoUploadTile = ({ tileWidth }: PendingVideoUploadTileProps) => {
  const colorPalette = useColorPalette();

  return (
    <View
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
    </View>
  );
};

export default PendingVideoUploadTile;
