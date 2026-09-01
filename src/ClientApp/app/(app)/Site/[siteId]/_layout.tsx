import { Stack } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function SiteLayout() {
  const colorPalette = useColorPalette();

  return (
    <Stack
      screenOptions={{
        headerShown: false,
        headerTintColor: colorPalette.primary,
      }}
    >
      <Stack.Screen
        name="(tabs)"
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
      <Stack.Screen name="Images" />
      <Stack.Screen name="Videos" />
      <Stack.Screen name="Invoices" />
      <Stack.Screen name="Issues" />
      <Stack.Screen
        name="People"
        options={{ headerShown: true, title: "People On Site" }}
      />
      <Stack.Screen
        name="Payments"
        options={{ headerShown: true, title: "Payments" }}
      />
    </Stack>
  );
}
