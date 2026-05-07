import { Text, View } from "react-native";

import { siteImagesStyles } from "./SiteImages.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const EmptyImageItem = () => {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[
        siteImagesStyles.emptyState,
        { borderColor: colorPalette.secondary + "55" },
      ]}
    >
      <Text
        style={[siteImagesStyles.emptyText, { color: colorPalette.secondary }]}
      >
        No images in this filter yet.
      </Text>
    </View>
  );
};

export default EmptyImageItem;
