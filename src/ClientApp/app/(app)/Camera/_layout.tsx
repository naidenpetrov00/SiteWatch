import { Stack } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";
import { Platform } from "react-native";

const CameraLayout = () => {
  const colorPalette = useColorPalette();

  return (
    <Stack
      screenOptions={{
        headerShown: true,
        headerTitle: "Camera",
        freezeOnBlur: true,
        headerTintColor: colorPalette.primary,
        ...(Platform.OS === "ios"
          ? {
              headerTransparent: true,
              headerBlurEffect: "systemUltraThinMaterial",
              headerShadowVisible: false,
            }
          : {}),
      }}
    ></Stack>
  );
};

export default CameraLayout;
