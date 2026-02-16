import { Href, router } from "expo-router";
import { Pressable, Text, View } from "react-native";

import React from "react";
import authPageTitleStyles from "./AuthPageTitle.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IAuthPageTitleProps {
  title: string;
  description: string;
  href?: Href;
  linkLabel?: string;
}

export const AuthPageTitle: React.FC<IAuthPageTitleProps> = ({
  title,
  description,
  href,
  linkLabel,
}) => {
  const colorPalette = useColorPalette();
  return (
    <View style={authPageTitleStyles.headerWrap}>
      <Text style={[authPageTitleStyles.title, { color: colorPalette.text }]}>
        {title}
      </Text>
      <View
        style={[
          authPageTitleStyles.subtitle,
        ]}
      >
        {href && linkLabel ? (
          <Text
            style={[
              authPageTitleStyles.subtitleText,
              { color: colorPalette.placeholderText },
            ]}
          >
            {description}{" "}
            <Text
              onPress={() => router.replace(href)}
              style={[
                authPageTitleStyles.subtitleText,
                authPageTitleStyles.subtitleLinkText,
                { color: colorPalette.primary },
              ]}
            >
              {linkLabel}
            </Text>
          </Text>
        ) : href ? (
          <Pressable onPress={() => router.replace(href)}>
            <Text
              style={[
                authPageTitleStyles.subtitleText,
                authPageTitleStyles.subtitleLinkText,
                { color: colorPalette.primary },
              ]}
            >
              {description}
            </Text>
          </Pressable>
        ) : (
          <Text
            style={[
              authPageTitleStyles.subtitleText,
              { color: colorPalette.secondary },
            ]}
          >
            {description}
          </Text>
        )}
      </View>
    </View>
  );
};

export default AuthPageTitle;
