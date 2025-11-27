import { Stack } from "expo-router";

export default function RootLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      {/*<Stack.Screen name="(app)" />*/}
      <Stack.Screen
        name="Site/[siteId]"
        options={{
          headerShown: true,
          title: "Site",
          headerBackTitle: "Sites",
        }}
      />
    </Stack>
  );
}
