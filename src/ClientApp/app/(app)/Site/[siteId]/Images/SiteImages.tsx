import {
  FlatList,
  View,
  useWindowDimensions,
} from "react-native";
import {
  GRID_GAP,
  HORIZONTAL_PADDING,
  siteImagesStyles,
} from "@/features/sites/info/images/component/SiteImages.styles";
import React, { useMemo, useState } from "react";

import EmptyImageItem from "@/features/sites/info/images/component/EmptyImageItems";
import { FilterType } from "@/features/sites/info/images/component/types";
import Filters from "@/features/sites/info/images/component/Filters/Filters";
import { GALLERY_ITEMS } from "./dummyValues";
import Header from "@/features/sites/info/images/component/Header/Header";
import ImageItem from "@/features/sites/info/images/component/ImageItem/ImageItem";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useSafeAreaInsets } from "react-native-safe-area-context";

const SiteImages = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const { width } = useWindowDimensions();
  const [activeFilter, setActiveFilter] = useState<FilterType>("All");

  const numColumns = width >= 900 ? 4 : width >= 600 ? 3 : 2;
  
  const tileWidth =
    (width - HORIZONTAL_PADDING * 2 - GRID_GAP * (numColumns - 1)) / numColumns;

  const filteredImages = useMemo(() => {
    if (activeFilter === "All") {
      return GALLERY_ITEMS;
    }

    return GALLERY_ITEMS.filter((item) => item.category === activeFilter);
  }, [activeFilter]);

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

      <FlatList
        data={filteredImages}
        key={`${numColumns}-${activeFilter}`}
        keyExtractor={(item) => item.id}
        numColumns={numColumns}
        showsVerticalScrollIndicator={false}
        columnWrapperStyle={
          numColumns > 1 ? siteImagesStyles.columnWrapper : undefined
        }
        contentContainerStyle={[
          siteImagesStyles.galleryContent,
          { paddingBottom: insets.bottom + 24 },
        ]}
        renderItem={({ item }) => (
         <ImageItem tileWidth={tileWidth} item={item}/>
        )}
        ListEmptyComponent={
          <EmptyImageItem/>
        }
      />
    </View>
  );
};

export default SiteImages;
