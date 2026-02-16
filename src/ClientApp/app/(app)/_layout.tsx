import { Stack } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function RootLayout() {
  const colorPalette = useColorPalette();

  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen
        name="Site/[siteId]"
        options={({ route }) => ({
          headerShown: true,
          title:
            ((route.params as { siteName?: string } | undefined)?.siteName ??
              "Site"),
          headerBackTitle: "Sites",
          headerBackButtonDisplayMode: "default",
          headerTintColor: colorPalette.primary,
        })}
      />
    </Stack>
  );
}
