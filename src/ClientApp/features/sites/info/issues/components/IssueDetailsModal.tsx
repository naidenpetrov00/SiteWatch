import Ionicons from "@expo/vector-icons/Ionicons";
import { ActivityIndicator, Modal, Pressable, ScrollView, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useColorPalette } from "@/hooks/useColorPalette";
import { formatIssueDate, formatIssueDateTime, formatIssueText } from "../formatters";
import { useGetIssueById } from "../hooks/useGetIssueById";
import type { SiteIssue } from "../types";
import issueDetailsModalStyles from "./IssueDetailsModal.styles";

type IssueDetailsModalProps = {
  issueId: string | null;
  onClose: () => void;
};

type DetailItem = {
  label: string;
  value: string;
};

const workerDisplayName = (worker: SiteIssue["assignedWorkers"][number]) =>
  worker.userName?.trim() || worker.email?.trim() || worker.id;

const IssueDetailsModal = ({ issueId, onClose }: IssueDetailsModalProps) => {
  const colorPalette = useColorPalette();
  const { data: issue, error, isError, isLoading } = useGetIssueById({
    issueId: issueId ?? undefined,
  });

  if (!issueId) return null;

  const renderBody = () => {
    if (isLoading) {
      return (
        <View style={issueDetailsModalStyles.state}>
          <ActivityIndicator color={colorPalette.primary} />
          <Text style={[issueDetailsModalStyles.stateText, { color: colorPalette.secondary }]}>Loading issue details…</Text>
        </View>
      );
    }

    if (isError || !issue) {
      const message = error instanceof Error ? error.message : "Issue details could not be retrieved.";
      return (
        <View style={issueDetailsModalStyles.state}>
          <Text style={[issueDetailsModalStyles.stateText, { color: colorPalette.text }]}>{message}</Text>
        </View>
      );
    }

    const sections: readonly { title: string; items: readonly DetailItem[] }[] = [
      {
        title: "Issue",
        items: [
          { label: "Reference", value: `#${issue.numberId}` },
          { label: "Status", value: issue.status },
          { label: "Description", value: issue.description },
          { label: "Start date", value: formatIssueDate(issue.startDate) },
          { label: "End date", value: formatIssueDate(issue.endDate) },
        ],
      },
      {
        title: "Assigned workers",
        items: [
          {
            label: "Workers",
            value: issue.assignedWorkers.length
              ? issue.assignedWorkers.map(workerDisplayName).join(", ")
              : "—",
          },
        ],
      },
      {
        title: "Activity",
        items: [
          { label: "Created", value: formatIssueDateTime(issue.created) },
          { label: "Created by", value: formatIssueText(issue.createdBy) },
          { label: "Last modified", value: formatIssueDateTime(issue.lastModified) },
          { label: "Last modified by", value: formatIssueText(issue.lastModifiedBy) },
        ],
      },
    ];

    return (
      <ScrollView contentContainerStyle={issueDetailsModalStyles.content}>
        {sections.map((section) => (
          <View key={section.title} style={issueDetailsModalStyles.section}>
            <Text style={[issueDetailsModalStyles.sectionTitle, { color: colorPalette.text }]}>{section.title}</Text>
            {section.items.map((item) => (
              <View
                key={item.label}
                style={[issueDetailsModalStyles.detailRow, { borderBottomColor: `${colorPalette.secondary}44` }]}
              >
                <Text style={[issueDetailsModalStyles.label, { color: colorPalette.secondary }]}>{item.label}</Text>
                <Text selectable style={[issueDetailsModalStyles.value, { color: colorPalette.text }]}>{item.value}</Text>
              </View>
            ))}
          </View>
        ))}
      </ScrollView>
    );
  };

  return (
    <Modal
      allowSwipeDismissal
      animationType="slide"
      onRequestClose={onClose}
      presentationStyle="pageSheet"
      visible
    >
      <SafeAreaView
        edges={["top", "bottom"]}
        style={[issueDetailsModalStyles.container, { backgroundColor: colorPalette.background }]}
      >
        <View style={[issueDetailsModalStyles.header, { borderBottomColor: `${colorPalette.secondary}55` }]}>
          <View style={issueDetailsModalStyles.heading}>
            <Text style={[issueDetailsModalStyles.eyebrow, { color: colorPalette.primary }]}>Issue details</Text>
            <Text style={[issueDetailsModalStyles.title, { color: colorPalette.text }]}>{issue?.title ?? "Issue"}</Text>
          </View>
          <Pressable
            accessibilityLabel="Close issue details"
            accessibilityRole="button"
            onPress={onClose}
            style={({ pressed }) => [
              issueDetailsModalStyles.closeButton,
              { backgroundColor: `${colorPalette.primary}18`, opacity: pressed ? 0.75 : 1 },
            ]}
          >
            <Ionicons color={colorPalette.primary} name="close" size={25} />
          </Pressable>
        </View>
        {renderBody()}
      </SafeAreaView>
    </Modal>
  );
};

export default IssueDetailsModal;
