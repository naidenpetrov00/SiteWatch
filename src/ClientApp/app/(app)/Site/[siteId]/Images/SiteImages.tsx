import { View } from "react-native";
import {
  siteImagesStyles,
} from "@/features/sites/info/images/component/SiteImages.styles";
import React, { useState } from "react";

import { FilterType } from "@/features/sites/info/images/component/types";
import Filters from "@/features/sites/info/images/component/Filters/Filters";
import Header from "@/features/sites/info/images/component/Header/Header";
import Images from "@/features/sites/info/images/component/Images/Images";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";

const SiteImages = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();

  const [activeFilter, setActiveFilter] = useState<FilterType>("All");

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

      <Filters activeFilter={activeFilter} setActiveFilter={setActiveFilter} />
      <Images activeFilter={activeFilter} siteId={siteId} />
    </View>
  );
};

export default SiteImages;
