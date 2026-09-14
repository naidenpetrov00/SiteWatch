import FloatingTabs from "@/components/app/FloatingTabs";

const IssueTabsLayout = () => {
  return <FloatingTabs tabs={[
    { androidIcon: { selected: "information-circle", unselected: "information-circle-outline" }, iosIcon: "info.circle", label: "Details", name: "index" },
    { androidIcon: { selected: "attach", unselected: "attach-outline" }, iosIcon: "paperclip", label: "Attachments", name: "attachments" },
  ]} />;
};

export default IssueTabsLayout;
