import { Text, View } from "react-native";

import React from "react";
import logoStyles from "./Logo.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface ILogoProps {}

const Logo: React.FC<ILogoProps> = ({}) => {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[logoStyles.logoBadge, { backgroundColor: colorPalette.primary }]}
    >
      <Text style={[logoStyles.logoText, { color: colorPalette.background }]}>
        Logo
      </Text>
    </View>
  );
};

export default Logo;
