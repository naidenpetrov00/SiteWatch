import { FlatList, useWindowDimensions } from "react-native";
import {
  GRID_GAP,
  HORIZONTAL_PADDING,
  siteImagesStyles,
} from "../SiteImages.styles";

import EmptyImageItem from "../EmptyImageItems";
import { FilterType } from "../types";
import ImageItem from "../ImageItem/ImageItem";
import { useGetSiteImageIdsBySiteId } from "../../hooks/useGetSiteImageIdsBySiteId";
import { useMemo } from "react";
import { useSafeAreaInsets } from "react-native-safe-area-context";

interface IImages {
  activeFilter: FilterType;
  siteId?: string;
}

const Images = ({ activeFilter, siteId }: IImages) => {
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();
  const { data: siteImageIds = [] } = useGetSiteImageIdsBySiteId({ siteId });
  const numColumns = width >= 900 ? 4 : width >= 600 ? 3 : 2;
  const tileWidth =
    (width - HORIZONTAL_PADDING * 2 - GRID_GAP * (numColumns - 1)) / numColumns;

  const filteredImages = useMemo(() => {
    if (activeFilter === "All") {
      return siteImageIds;
    }

    return [];
  }, [activeFilter, siteImageIds]);

  return (
    <FlatList
      data={filteredImages}
      key={`${numColumns}-${activeFilter}`}
      keyExtractor={(item) => item.imageId}
      numColumns={numColumns}
      showsVerticalScrollIndicator={false}
      columnWrapperStyle={
        numColumns > 1 ? siteImagesStyles.columnWrapper : undefined
      }
      contentContainerStyle={[
        siteImagesStyles.galleryContent,
        { paddingBottom: insets.bottom + 24 },
      ]}
      renderItem={({ item }) => <ImageItem tileWidth={tileWidth} item={item} />}
      ListEmptyComponent={<EmptyImageItem />}
    />
  );
};

export default Images;
