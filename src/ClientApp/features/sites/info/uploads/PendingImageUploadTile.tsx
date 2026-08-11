import { ActivityIndicator, Image, Text, View } from "react-native";
import Animated, { FadeInDown } from "react-native-reanimated";

import { useColorPalette } from "@/hooks/useColorPalette";
import { siteImagesStyles } from "../images/component/SiteImages.styles";

type PendingImageUploadTileProps = {
  tileWidth: number;
  uri: string;
};

const PendingImageUploadTile = ({
  tileWidth,
  uri,
}: PendingImageUploadTileProps) => {
  const colorPalette = useColorPalette();

  return (
    <Animated.View
      entering={FadeInDown.duration(180)}
      accessibilityLabel="Image uploading"
      style={[
        siteImagesStyles.galleryTile,
        { width: tileWidth, backgroundColor: `${colorPalette.primary}22` },
      ]}
    >
      <Image
        source={{ uri }}
        resizeMode="cover"
        style={siteImagesStyles.galleryImage}
      />
      <View
        style={[
          siteImagesStyles.tilePlaceholder,
          { backgroundColor: "rgba(0, 0, 0, 0.42)" },
        ]}
      >
        <ActivityIndicator color={colorPalette.contrastText} />
        <Text
          style={[
            siteImagesStyles.tilePlaceholderText,
            { color: colorPalette.contrastText },
          ]}
        >
          Uploading…
        </Text>
      </View>
    </Animated.View>
  );
};

export default PendingImageUploadTile;
