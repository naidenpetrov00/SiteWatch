import { Stack } from "expo-router";
import RoleRouteGuard from "@/features/auth/components/RoleRouteGuard/RoleRouteGuard";
import { ACCESS_POLICIES } from "@/types/authorization";

export default function RootLayout() {
  return (
    <RoleRouteGuard allowedRoles={ACCESS_POLICIES.currentApp}>
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="Site/[siteId]" />
      </Stack>
    </RoleRouteGuard>
  );
}
