import { Pressable, Text, View } from "react-native";

import React from "react";
import authPageTitleStyles from "./AuthPageTitle.styles";
import { router } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IAuthPageTitleProps {
  title: string;
  description: string;
}

export const AuthPageTitle: React.FC<IAuthPageTitleProps> = ({
  title,
  description,
}) => {
  const colorPalette = useColorPalette();

  return (
    <View style={authPageTitleStyles.headerWrap}>
      <Text style={[authPageTitleStyles.title, { color: colorPalette.text }]}>
        {title}
      </Text>
      <Text
        style={[
          authPageTitleStyles.subtitle,
          { color: colorPalette.secondary },
        ]}
      >
        {description}{" "}
        <Pressable onPress={() => router.push("/SignUp")}>
          <Text>here</Text>
        </Pressable>
      </Text>
    </View>
  );
};

export default AuthPageTitle;
