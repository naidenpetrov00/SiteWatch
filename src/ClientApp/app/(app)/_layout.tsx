import { Stack } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function RootLayout() {
  const colorPalette = useColorPalette();

  return (
    <Stack screenOptions={{ headerShown: false }}>
      {/*<Stack.Screen name="(app)" />*/}
      <Stack.Screen
        name="Site/[siteId]"
        options={{
          headerShown: true,
          title: "Site",
          headerBackTitle: "Sites",
          headerBackButtonDisplayMode: "default",
          headerStyle: { backgroundColor: "#111" },
          headerTintColor: colorPalette.primary,
        }}
      />
    </Stack>
  );
}
