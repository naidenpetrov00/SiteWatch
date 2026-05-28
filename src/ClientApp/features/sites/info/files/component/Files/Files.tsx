import { Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import filesStyles from "./Files.styles";

const Files = () => {
  const colorPalette = useColorPalette();

  return (
    <View style={filesStyles.container}>
      <View
        style={[
          filesStyles.emptyState,
          { borderColor: colorPalette.secondary + "55" },
        ]}
      >
        <Text style={[filesStyles.title, { color: colorPalette.text }]}>
          Files
        </Text>
        <Text
          style={[filesStyles.description, { color: colorPalette.secondary }]}
        >
          Files page shell is ready.
        </Text>
      </View>
    </View>
  );
};

export default Files;
