import React, { useEffect, useMemo, useState } from "react";
import { View } from "react-native";

import { FilterType } from "@/features/sites/info/videos/component/types";
import Filters from "@/features/sites/info/videos/component/Filters/Filters";
import Header from "@/features/sites/info/videos/component/Header/Header";
import Videos from "@/features/sites/info/videos/component/Videos/Videos";
import { siteVideosStyles } from "@/features/sites/info/videos/component/SiteVideos.styles";
import { ALL_FILTER } from "@/features/sites/info/media-types";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";

const SiteVideos = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const { data: sites = [] } = useGetSitesByUserId();

  const [activeFilter, setActiveFilter] = useState<FilterType>(ALL_FILTER);
  const site = useMemo(
    () => sites.find((siteItem) => siteItem.id === siteId),
    [siteId, sites],
  );
  const filters = useMemo<FilterType[]>(
    () => [ALL_FILTER, ...(site?.mediaPolicy.allowedVideoCategories ?? [])],
    [site],
  );
  const resolvedActiveFilter = filters.includes(activeFilter)
    ? activeFilter
    : ALL_FILTER;

  useEffect(() => {
    if (resolvedActiveFilter !== activeFilter) {
      setActiveFilter(ALL_FILTER);
    }
  }, [activeFilter, resolvedActiveFilter]);

  return (
    <View
      style={[
        siteVideosStyles.container,
        {
          backgroundColor: colorPalette.background,
        },
      ]}
    >
      <Header siteId={siteId} />
      <Filters
        activeFilter={resolvedActiveFilter}
        filters={filters}
        setActiveFilter={setActiveFilter}
      />
      <Videos activeFilter={resolvedActiveFilter} siteId={siteId} />
    </View>
  );
};

export default SiteVideos;
