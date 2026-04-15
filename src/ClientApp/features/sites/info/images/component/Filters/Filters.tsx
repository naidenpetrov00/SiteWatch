import { Pressable, Text, View } from "react-native";

import { FilterType } from "../types";
import { filtersStyles } from "./Filters.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

export const FILTERS = ["All", "Pipes", "Electical Scheme"] as const;

interface IFilters {
  activeFilter: FilterType;
  setActiveFilter: React.Dispatch<React.SetStateAction<FilterType>>;
}

const Filters = ({ activeFilter, setActiveFilter }: IFilters) => {
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
