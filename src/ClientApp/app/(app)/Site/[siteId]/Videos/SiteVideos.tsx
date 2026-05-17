import React, { useState } from "react";

import { FilterType } from "@/features/sites/info/videos/component/types";
import Filters from "@/features/sites/info/videos/component/Filters/Filters";
import Header from "@/features/sites/info/videos/component/Header/Header";
import Videos from "@/features/sites/info/videos/component/Videos/Videos";
import { View } from "react-native";
import { siteVideosStyles } from "@/features/sites/info/videos/component/SiteVideos.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";

const SiteVideos = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();

  const [activeFilter, setActiveFilter] = useState<FilterType>("All");

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
      <Filters activeFilter={activeFilter} setActiveFilter={setActiveFilter} />
      <Videos activeFilter={activeFilter} siteId={siteId} />
    </View>
  );
};

export default SiteVideos;