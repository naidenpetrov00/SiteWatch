import FloatingTabs from "@/components/app/FloatingTabs";
import { ACCESS_POLICIES } from "@/types/authorization";
import { useAuth } from "@/store/auth_context";

export default function SitesLayout() {
  const { hasAnyRole } = useAuth();

  return <FloatingTabs headerTintColor tabs={[
    { androidIcon: { selected: "business", unselected: "business-outline" }, iosIcon: "building.2", label: "Sites", name: "Sites" },
    { androidIcon: { selected: "home", unselected: "home-outline" }, iosIcon: "house", label: "Home", name: "index" },
    { androidIcon: { selected: "construct", unselected: "construct-outline" }, hidden: !hasAnyRole(ACCESS_POLICIES.assetTracking), iosIcon: "shippingbox", label: "Assets", name: "Assets" },
  ]} />;
}
