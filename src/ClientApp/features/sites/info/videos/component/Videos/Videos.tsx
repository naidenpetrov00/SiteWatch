import { FlatList, useWindowDimensions } from "react-native";
import {
  GRID_GAP,
  HORIZONTAL_PADDING,
  siteVideosStyles,
} from "./Videos.styles";
import { useCallback } from "react";

import EmptyVideoItem from "../EmptyVideoItems";
import { FilterType } from "../types";
import type { SiteVideoIds } from "../../types";
import VideoItem from "../VideoItem/VideoItem";
import { useGetSiteVideoIdsBySiteId } from "../../hooks/useGetSiteVideoIdsBySiteId";
import { getSiteVideoSnapshot } from "../../hooks/useGetSiteVideoSnapshot";
import { useQueries, useQueryClient } from "@tanstack/react-query";
import { useSafeAreaInsets } from "react-native-safe-area-context";

const MIN_TILE_WIDTH = 150;

type VisibleSiteVideo = SiteVideoIds & {
  snapshotUri: string;
};

interface IVideos {
  activeFilter: FilterType;
  siteId?: string;
}

const Videos = ({ activeFilter, siteId }: IVideos) => {
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();
  const queryClient = useQueryClient();
  const {
    data: siteVideoIds = [],
    isRefetching,
    refetch,
  } = useGetSiteVideoIdsBySiteId({ siteId });

  const availableWidth = width - HORIZONTAL_PADDING * 2;
  const numColumns = Math.max(
    1,
    Math.floor((availableWidth + GRID_GAP) / (MIN_TILE_WIDTH + GRID_GAP)),
  );
  const tileWidth = (availableWidth - GRID_GAP * (numColumns - 1)) / numColumns;

  const snapshotQueries = useQueries({
    queries: siteVideoIds.map((video) => ({
      queryKey: ["video-snapshot", video.snapshotId],
      enabled: Boolean(video.snapshotId) && activeFilter === "All",
      queryFn: () => getSiteVideoSnapshot({ snapshotId: video.snapshotId }),
      retry: false,
    })),
  });

  const filteredVideos =
    activeFilter === "All"
      ? siteVideoIds.flatMap<VisibleSiteVideo>((video, index) => {
          const snapshotQuery = snapshotQueries[index];

          if (!snapshotQuery || snapshotQuery.status !== "success") {
            return [];
          }

          return [
            {
              ...video,
              snapshotUri: snapshotQuery.data,
            },
          ];
        })
      : [];

  const isResolvingSnapshots =
    activeFilter === "All" &&
    siteVideoIds.length > 0 &&
    snapshotQueries.some((query) => query.isPending || query.isFetching);

  const showEmptyState = !isResolvingSnapshots && filteredVideos.length === 0;

  const handleRefresh = useCallback(async () => {
    await queryClient.invalidateQueries({
      queryKey: ["video-snapshot"],
    });
    await refetch();
  }, [queryClient, refetch]);

  return (
    <FlatList<VisibleSiteVideo>
      data={filteredVideos}
      key={`${numColumns}-${activeFilter}`}
      keyExtractor={(item) => item.videoId}
      numColumns={numColumns}
      showsVerticalScrollIndicator={false}
      columnWrapperStyle={
        numColumns > 1 ? siteVideosStyles.columnWrapper : undefined
      }
      contentContainerStyle={[
        siteVideosStyles.galleryContent,
        { paddingBottom: insets.bottom + 24 },
      ]}
      initialNumToRender={numColumns * 3}
      maxToRenderPerBatch={numColumns * 3}
      windowSize={5}
      renderItem={({ item }) => <VideoItem tileWidth={tileWidth} item={item} />}
      ListEmptyComponent={showEmptyState ? <EmptyVideoItem /> : null}
      refreshing={isRefetching}
      onRefresh={handleRefresh}
    />
  );
};

export default Videos;
