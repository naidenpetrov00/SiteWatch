import { Href, HrefObject, router } from "expo-router";
import { Pressable, Text, View } from "react-native";

import { NavigationOptions } from "expo-router/build/global-state/routing";
import React from "react";
import authPageTitleStyles from "./AuthPageTitle.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IAuthPageTitleProps {
  title: string;
  description: string;
  href?: Href;
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
        {href ? (
          <Pressable onPress={() => router.replace(href)}>
            <Text>{description}</Text>
          </Pressable>
        ) : (
          <Text>{description}</Text>
        )}
      </Text>
    </View>
  );
};

export default AuthPageTitle;
