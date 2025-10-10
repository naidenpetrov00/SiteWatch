import { Href, router } from "expo-router";
import { Pressable, Text, View } from "react-native";

import React from "react";
import authPageTitleStyles from "./AuthPageTitle.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IAuthPageTitleProps {
  title: string;
  description: string;
  href: Href;
}

export const AuthPageTitle: React.FC<IAuthPageTitleProps> = ({
  title,
  description,
  href,
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
        <Pressable onPress={() => router.push(href)}>
          <Text>{description}</Text>
        </Pressable>
      </Text>
    </View>
  );
};

export default AuthPageTitle;
