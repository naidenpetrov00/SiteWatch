import { ActivityIndicator, ScrollView, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { formatIssueDate, formatIssueDateTime, formatIssueText } from "../formatters";
import { useGetIssueById } from "../hooks/useGetIssueById";
import type { SiteIssue } from "../types";
import styles from "./IssueDetailsPage.styles";

const workerDisplayName = (worker: SiteIssue["assignedWorkers"][number]) => worker.userName?.trim() || worker.email?.trim() || worker.id;

const IssueDetailsContent = () => {
  const colorPalette = useColorPalette();
  const { issueId } = useGetSearchParams<{ issueId?: string }>();
  const issueQuery = useGetIssueById({ issueId });
  const issue = issueQuery.data;
  const sections: readonly { title: string; items: readonly { label: string; value: string }[] }[] = issue ? [
    { title: "Issue", items: [{ label: "Reference", value: `#${issue.numberId}` }, { label: "Status", value: issue.status }, { label: "Description", value: issue.description }, { label: "Start date", value: formatIssueDate(issue.startDate) }, { label: "End date", value: formatIssueDate(issue.endDate) }] },
    { title: "Assigned workers", items: [{ label: "Workers", value: issue.assignedWorkers.length ? issue.assignedWorkers.map(workerDisplayName).join(", ") : "—" }] },
    { title: "Activity", items: [{ label: "Created", value: formatIssueDateTime(issue.created) }, { label: "Created by", value: formatIssueText(issue.createdBy) }, { label: "Last modified", value: formatIssueDateTime(issue.lastModified) }, { label: "Last modified by", value: formatIssueText(issue.lastModifiedBy) }] },
  ] : [];

  return (
    <ScrollView contentContainerStyle={styles.content} contentInsetAdjustmentBehavior="automatic">
      {issueQuery.isLoading ? <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><ActivityIndicator color={colorPalette.primary} /><Text style={[styles.stateText, { color: colorPalette.secondary }]}>Loading issue details…</Text></View> : issue ? sections.map((section) => <View key={section.title} style={styles.section}><Text style={[styles.sectionTitle, { color: colorPalette.text }]}>{section.title}</Text>{section.items.map((item) => <View key={item.label} style={[styles.detailRow, { borderBottomColor: `${colorPalette.secondary}44` }]}><Text style={[styles.label, { color: colorPalette.secondary }]}>{item.label}</Text><Text selectable style={[styles.value, { color: colorPalette.text }]}>{item.value}</Text></View>)}</View>) : <View style={[styles.state, { borderColor: `${colorPalette.secondary}55` }]}><Text style={[styles.stateText, { color: colorPalette.text }]}>{issueQuery.error instanceof Error ? issueQuery.error.message : "Issue details could not be retrieved."}</Text></View>}
    </ScrollView>
  );
};

export default IssueDetailsContent;
