import { FlatList, useWindowDimensions } from "react-native";
import {
  GRID_GAP,
  HORIZONTAL_PADDING,
  siteImagesStyles,
} from "../SiteImages.styles";
import { useCallback, useMemo } from "react";

import EmptyImageItem from "../EmptyImageItems";
import { FilterType } from "../types";
import ImageItem from "../ImageItem/ImageItem";
import { useGetSiteImageIdsBySiteId } from "../../hooks/useGetSiteImageIdsBySiteId";
import { useQueryClient } from "@tanstack/react-query";
import { useSafeAreaInsets } from "react-native-safe-area-context";

const MIN_TILE_WIDTH = 150;

interface IImages {
  activeFilter: FilterType;
  siteId?: string;
}

const Images = ({ activeFilter, siteId }: IImages) => {
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();
  const queryClient = useQueryClient();
  const {
    data: siteImageIds = [],
    isRefetching,
    refetch,
  } = useGetSiteImageIdsBySiteId({ siteId });
  const availableWidth = width - HORIZONTAL_PADDING * 2;
  const numColumns = Math.max(
    1,
    Math.floor((availableWidth + GRID_GAP) / (MIN_TILE_WIDTH + GRID_GAP)),
  );
  const tileWidth =
    (availableWidth - GRID_GAP * (numColumns - 1)) / numColumns;

  const filteredImages = useMemo(() => {
    if (activeFilter === "All") {
      return siteImageIds;
    }

    return [];
  }, [activeFilter, siteImageIds]);

  const handleRefresh = useCallback(async () => {
    await queryClient.invalidateQueries({
      queryKey: ["site-image-thumbnail"],
    });
    await refetch();
  }, [queryClient, refetch]);

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
      initialNumToRender={numColumns * 3}
      maxToRenderPerBatch={numColumns * 3}
      windowSize={5}
      renderItem={({ item }) => <ImageItem tileWidth={tileWidth} item={item} />}
      ListEmptyComponent={<EmptyImageItem />}
      refreshing={isRefetching}
      onRefresh={handleRefresh}
    />
  );
};

export default Images;
