import { Text, View } from "react-native";

import { SiteImageIds } from "../../types";
import { siteImagesStyles } from "../SiteImages.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IImageItem {
  tileWidth: number;
  item: SiteImageIds;
}

const ImageItem = ({ tileWidth, item }: IImageItem) => {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[
        siteImagesStyles.galleryTile,
        {
          width: tileWidth,
          backgroundColor: `${colorPalette.primary}22`,
        },
      ]}
    >
      <View style={siteImagesStyles.tileOverlay}>
        <Text
          style={[siteImagesStyles.tileCategory, { color: colorPalette.text }]}
        >
          Thumbnail ID
        </Text>
        <Text
          style={[siteImagesStyles.tileTitle, { color: colorPalette.text }]}
          numberOfLines={2}
        >
          {item.thumbnailId}
        </Text>
      </View>
    </View>
  );
};

export default ImageItem;
