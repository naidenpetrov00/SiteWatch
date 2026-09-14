import FloatingTabs from "@/components/app/FloatingTabs";

export default function SitesLayout() {
  return <FloatingTabs headerTintColor tabs={[
    { androidIcon: { selected: "business", unselected: "business-outline" }, iosIcon: "building.2", label: "Sites", name: "Sites" },
    { androidIcon: { selected: "home", unselected: "home-outline" }, iosIcon: "house", label: "Home", name: "index" },
  ]} />;
}
