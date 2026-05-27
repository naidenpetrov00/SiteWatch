import React, { useEffect, useMemo, useState } from "react";
import { View } from "react-native";

import { siteImagesStyles } from "@/features/sites/info/images/component/SiteImages.styles";

import { FilterType } from "@/features/sites/info/images/component/types";
import Filters from "@/features/sites/info/images/component/Filters/Filters";
import Header from "@/features/sites/info/images/component/Header/Header";
import Images from "@/features/sites/info/images/component/Images/Images";
import { ALL_FILTER } from "@/features/sites/info/media-types";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";

const SiteImages = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const { data: sites = [] } = useGetSitesByUserId();

  const [activeFilter, setActiveFilter] = useState<FilterType>(ALL_FILTER);
  const site = useMemo(
    () => sites.find((siteItem) => siteItem.id === siteId),
    [siteId, sites],
  );
  const filters = useMemo<FilterType[]>(
    () => [ALL_FILTER, ...(site?.mediaPolicy.allowedImageCategories ?? [])],
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
        siteImagesStyles.container,
        {
          backgroundColor: colorPalette.background,
          // paddingTop: (insets.top || 0) + 16,
        },
      ]}
    >
      <Header siteId={siteId} />

      <Filters
        activeFilter={resolvedActiveFilter}
        filters={filters}
        setActiveFilter={setActiveFilter}
      />
      <Images activeFilter={resolvedActiveFilter} siteId={siteId} />
    </View>
  );
};

export default SiteImages;
