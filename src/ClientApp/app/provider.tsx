import "react-native-reanimated";

import * as SplashScreen from "expo-splash-screen";

import { DarkTheme, DefaultTheme } from "@react-navigation/native";
import React, { ReactNode } from "react";

import { ThemeProvider } from "@react-navigation/native";
import { useColorScheme } from "@/hooks/useColorScheme";
import { useEffect } from "react";
import { useFonts } from "expo-font";

type AppProviderProps = {
  children: ReactNode;
};

export const AppProvider = ({ children }: AppProviderProps) => {
  const colorScheme = useColorScheme();
  const [loaded] = useFonts({
    SpaceMono: require("../assets/fonts/SpaceMono-Regular.ttf"),
  });

  useEffect(() => {
    if (loaded) {
      SplashScreen.hideAsync();
    }
  }, [loaded]);

  if (!loaded) {
    return null;
  }

  return (
    <ThemeProvider value={colorScheme === "dark" ? DarkTheme : DefaultTheme}>
      {children}
    </ThemeProvider>
  );
};
