import { View } from "react-native";

import LoginPageLogo from "@/assets/logo/LoginPageLogo";
import React from "react";
import logoStyles from "./Logo.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const Logo: React.FC = () => {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[logoStyles.logoBadge, { backgroundColor: colorPalette.primary }]}
    >
      <LoginPageLogo />
    </View>
  );
};

export default Logo;
