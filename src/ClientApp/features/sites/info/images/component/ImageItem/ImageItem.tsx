import { ActivityIndicator, Image, Text, View } from "react-native";

import type { SiteImageIds } from "../../types";
import { siteImagesStyles } from "../SiteImages.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetSiteImageThumbnail } from "../../hooks/useGetSiteImageThumbnail";

interface IImageItem {
  tileWidth: number;
  item: SiteImageIds;
}

const ImageItem = ({ tileWidth, item }: IImageItem) => {
  const colorPalette = useColorPalette();
  const {
    data: thumbnailUri,
    isError,
    isLoading,
  } = useGetSiteImageThumbnail({
    imageId: item.thumbnailId,
  });

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
      {thumbnailUri ? (
        <Image
          source={{ uri: thumbnailUri }}
          resizeMode="cover"
          style={siteImagesStyles.galleryImage}
        />
      ) : null}

      {isLoading ? (
        <View style={siteImagesStyles.tilePlaceholder}>
          <ActivityIndicator color={colorPalette.primary} />
        </View>
      ) : null}

      {isError ? (
        <View style={siteImagesStyles.tilePlaceholder}>
          <Text
            style={[
              siteImagesStyles.tilePlaceholderText,
              { color: colorPalette.secondary },
            ]}
          >
            Image unavailable
          </Text>
        </View>
      ) : null}
    </View>
  );
};

export default ImageItem;
