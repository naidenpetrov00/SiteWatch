import { FlatList, useWindowDimensions } from "react-native";
import {
  GRID_GAP,
  HORIZONTAL_PADDING,
  siteVideosStyles,
} from "../SiteVideos.styles";

import EmptyVideoItem from "../EmptyVideoItems";
import { FilterType } from "../types";
import { useMemo } from "react";
import { useSafeAreaInsets } from "react-native-safe-area-context";

const MIN_TILE_WIDTH = 150;

interface IVideos {
  activeFilter: FilterType;
  siteId?: string;
}

const Videos = ({ activeFilter, siteId }: IVideos) => {
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();

  const availableWidth = width - HORIZONTAL_PADDING * 2;
  const numColumns = Math.max(
    1,
    Math.floor((availableWidth + GRID_GAP) / (MIN_TILE_WIDTH + GRID_GAP)),
  );

  const filteredVideos = useMemo(() => {
    // Placeholder: no real video data yet
    return [];
  }, [activeFilter, siteId]);

  return (
    <FlatList
      data={filteredVideos}
      key={`${numColumns}-${activeFilter}`}
      keyExtractor={(item) => item}
      numColumns={numColumns}
      showsVerticalScrollIndicator={false}
      columnWrapperStyle={
        numColumns > 1 ? siteVideosStyles.columnWrapper : undefined
      }
      contentContainerStyle={[
        siteVideosStyles.galleryContent,
        { paddingBottom: insets.bottom + 24 },
      ]}
      renderItem={() => null}
      ListEmptyComponent={<EmptyVideoItem />}
    />
  );
};

export default Videos;