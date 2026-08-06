import { Stack } from "expo-router";

import RoleRouteGuard from "@/features/auth/components/RoleRouteGuard/RoleRouteGuard";
import { ACCESS_POLICIES } from "@/types/authorization";

const InvoicesLayout = () => (
  <RoleRouteGuard allowedRoles={ACCESS_POLICIES.siteInvoices}>
    <Stack>
      <Stack.Screen name="index" options={{ title: "Invoices" }} />
    </Stack>
  </RoleRouteGuard>
);

export default InvoicesLayout;
