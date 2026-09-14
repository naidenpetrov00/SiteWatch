import FloatingTabs from "@/components/app/FloatingTabs";

export default function SiteTabsLayout() {
  return <FloatingTabs tabs={[
    { androidIcon: { selected: "camera", unselected: "camera-outline" }, iosIcon: "camera", label: "Cameras", name: "Cameras" },
    { androidIcon: { selected: "information-circle", unselected: "information-circle-outline" }, iosIcon: "info.circle", label: "Info", name: "Info" },
  ]} />;
}
