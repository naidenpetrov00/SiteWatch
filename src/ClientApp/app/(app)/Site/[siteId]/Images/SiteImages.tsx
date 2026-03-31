import {
  FlatList,
  Pressable,
  Text,
  View,
  useWindowDimensions,
} from "react-native";
import { GRID_GAP, HORIZONTAL_PADDING, siteImagesStyles } from "@/features/sites/info/images/component/index.styles";
import React, { useMemo, useState } from "react";

import { FilterType } from "@/features/sites/info/images/component/types";
import { GALLERY_ITEMS } from "./dummyValues";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useSafeAreaInsets } from "react-native-safe-area-context";

export const FILTERS = ["All", "Pipes", "Electical Scheme"] as const;



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
      <Text style={[siteImagesStyles.title, { color: colorPalette.text }]}>
        Site Images
      </Text>
      <Text style={[siteImagesStyles.subtitle, { color: colorPalette.secondary }]}>
        Site ID: {siteId ?? "Unknown"}
      </Text>

      <View style={siteImagesStyles.filters}>
        {FILTERS.map((filter) => {
          const isActive = filter === activeFilter;

          return (
            <Pressable
              key={filter}
              onPress={() => setActiveFilter(filter)}
              style={[
                siteImagesStyles.filterChip,
                {
                  backgroundColor: isActive
                    ? colorPalette.primary
                    : colorPalette.background,
                  borderColor: isActive
                    ? colorPalette.primary
                    : colorPalette.secondary,
                },
              ]}
            >
              <Text
                style={[
                  siteImagesStyles.filterText,
                  {
                    color: isActive
                      ? colorPalette.contrastText
                      : colorPalette.text,
                  },
                ]}
              >
                {filter}
              </Text>
            </Pressable>
          );
        })}
      </View>

      <FlatList
        data={filteredImages}
        key={`${numColumns}-${activeFilter}`}
        keyExtractor={(item) => item.id}
        numColumns={numColumns}
        showsVerticalScrollIndicator={false}
        columnWrapperStyle={numColumns > 1 ? siteImagesStyles.columnWrapper : undefined}
        contentContainerStyle={[
          siteImagesStyles.galleryContent,
          { paddingBottom: insets.bottom + 24 },
        ]}
        renderItem={({ item }) => (
          <View
            style={[
              siteImagesStyles.galleryTile,
              {
                width: tileWidth,
                backgroundColor: item.color,
              },
            ]}
          >
            <View style={siteImagesStyles.tileOverlay}>
              <Text style={[siteImagesStyles.tileCategory, { color: colorPalette.text }]}>
                {item.category}
              </Text>
              <Text style={[siteImagesStyles.tileTitle, { color: colorPalette.text }]}>
                {item.title}
              </Text>
            </View>
          </View>
        )}
        ListEmptyComponent={
          <View
            style={[
              siteImagesStyles.emptyState,
              { borderColor: colorPalette.secondary + "55" },
            ]}
          >
            <Text style={[siteImagesStyles.emptyText, { color: colorPalette.secondary }]}>
              No images in this filter yet.
            </Text>
          </View>
        }
      />
    </View>
  );
};


export default SiteImages;
