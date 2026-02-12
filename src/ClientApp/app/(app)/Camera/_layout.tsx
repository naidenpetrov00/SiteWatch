import { Stack } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

const CameraLayout = () => {
  const colorPalette = useColorPalette();

  return (
    <Stack
      screenOptions={{
        headerShown: true,
        headerTitle: "Camera",
        freezeOnBlur: true,
        headerTintColor: colorPalette.primary,
      }}
    ></Stack>
  );
};

export default CameraLayout;
