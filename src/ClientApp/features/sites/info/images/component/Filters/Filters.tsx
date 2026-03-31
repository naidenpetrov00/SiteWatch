import { View } from "react-native";
import { filtersStyles } from "./Filters.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useEffect } from "react";

interface IFilters {
  test: string;
}

const Filters = ({ test }: IFilters) => {
  const colorPalette = useColorPalette();

  return (
    <View style={filtersStyles.filters}>
      {FILTERS.map((filter) => {
        const isActive = filter === activeFilter;

        return (
          <Pressable
            key={filter}
            onPress={() => setActiveFilter(filter)}
            style={[
              filtersStyles.filterChip,
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
                filtersStyles.filterText,
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
  );
};

export default Filters;
