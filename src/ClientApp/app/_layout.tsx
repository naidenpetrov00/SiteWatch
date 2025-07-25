import "react-native-reanimated";

import * as SplashScreen from "expo-splash-screen";

import { AppProvider } from "./provider";
import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  return (
    <AppProvider>
      <Stack>
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        <Stack.Screen name="+not-found" />
      </Stack>
      <StatusBar style="auto" />
    </AppProvider>
  );
}
