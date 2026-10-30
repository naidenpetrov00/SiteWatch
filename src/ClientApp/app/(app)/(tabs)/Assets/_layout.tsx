import { Stack } from "expo-router";

import RoleRouteGuard from "@/features/auth/components/RoleRouteGuard/RoleRouteGuard";
import { ACCESS_POLICIES } from "@/types/authorization";

export default function AssetsLayout() {
  return (
    <RoleRouteGuard allowedRoles={ACCESS_POLICIES.assetTracking}>
      <Stack>
        <Stack.Screen name="index" options={{ title: "Assets" }} />
        <Stack.Screen name="scan" options={{ title: "Scan asset" }} />
        <Stack.Screen name="search" options={{ title: "Search assets" }} />
        <Stack.Screen name="register" options={{ title: "Register asset" }} />
        <Stack.Screen name="[id]" options={{ title: "Asset details" }} />
        <Stack.Screen name="registrations/[id]" options={{ title: "Registration status" }} />
      </Stack>
    </RoleRouteGuard>
  );
}
