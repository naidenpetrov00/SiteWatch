import Ionicons from "@expo/vector-icons/Ionicons";
import { memo, useCallback, useState } from "react";
import { ActivityIndicator, FlatList, Pressable, Text, View } from "react-native";

import RoleGate from "@/features/auth/components/RoleGate/RoleGate";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { ACCESS_POLICIES } from "@/types/authorization";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { useGetSiteIssues } from "../hooks/useGetSiteIssues";
import type { SiteIssue } from "../types";
import AddIssueModal from "./AddIssueModal";
import IssueDetailsModal from "./IssueDetailsModal";
import issuesCardStyles from "./IssuesCard.styles";

type IssueRowProps = {
  issue: SiteIssue;
  onSelect: (issueId: string) => void;
};

const IssueRow = memo(function IssueRow({ issue, onSelect }: IssueRowProps) {
  const colorPalette = useColorPalette();

  return (
    <Pressable
      accessibilityLabel={`View issue ${issue.title}`}
      accessibilityRole="button"
      onPress={() => onSelect(issue.id)}
      style={({ pressed }) => [
        issuesCardStyles.row,
        {
          backgroundColor: colorPalette.background,
          borderColor: `${colorPalette.secondary}55`,
          opacity: pressed ? 0.75 : 1,
        },
      ]}
    >
      <View style={issuesCardStyles.rowHeader}>
        <Text numberOfLines={1} style={[issuesCardStyles.issueTitle, { color: colorPalette.text }]}>{issue.title}</Text>
        <Text style={[issuesCardStyles.status, { backgroundColor: `${colorPalette.primary}1A`, color: colorPalette.primary }]}>{issue.status}</Text>
      </View>
      <Text style={[issuesCardStyles.reference, { color: colorPalette.secondary }]}>Issue #{issue.numberId}</Text>
    </Pressable>
  );
});

const IssuesCard = () => {
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const [selectedIssueId, setSelectedIssueId] = useState<string | null>(null);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const {
    data: issues = [],
    error,
    isError,
    isLoading,
    isRefetching,
    refetch,
  } = useGetSiteIssues({ siteId });

  const selectIssue = useCallback((issueId: string) => {
    setSelectedIssueId(issueId);
  }, []);
  const renderIssue = useCallback(
    ({ item }: { item: SiteIssue }) => <IssueRow issue={item} onSelect={selectIssue} />,
    [selectIssue],
  );
  const keyExtractor = useCallback((issue: SiteIssue) => issue.id, []);

  const stateMessage = isLoading
    ? "Loading issues…"
    : isError
      ? error instanceof Error
        ? error.message
        : "Issues could not be retrieved."
      : "No issues have been reported for this site.";

  if (!siteId) return null;

  return (
    <View style={[issuesCardStyles.container, { backgroundColor: colorPalette.background }]}>
      <View style={issuesCardStyles.header}>
        <View style={issuesCardStyles.heading}>
          <Text style={[issuesCardStyles.title, { color: colorPalette.text }]}>Site Issues</Text>
          <Text style={[issuesCardStyles.subtitle, { color: colorPalette.secondary }]}>Review and report site issues.</Text>
        </View>
        <RoleGate allowedRoles={ACCESS_POLICIES.issueCreation}>
          <Pressable
            accessibilityLabel="Add issue"
            accessibilityRole="button"
            onPress={() => setIsAddModalOpen(true)}
            style={({ pressed }) => [
              issuesCardStyles.addButton,
              { backgroundColor: colorPalette.primary, opacity: pressed ? 0.78 : 1 },
            ]}
          >
            <Ionicons color={colorPalette.background} name="add" size={18} />
            <Text style={[issuesCardStyles.addButtonText, { color: colorPalette.background }]}>Add</Text>
          </Pressable>
        </RoleGate>
      </View>
      <FlatList
        contentContainerStyle={[
          issuesCardStyles.list,
          { paddingBottom: insets.bottom + 24 },
        ]}
        contentInsetAdjustmentBehavior="automatic"
        data={isError || isLoading ? [] : issues}
        keyExtractor={keyExtractor}
        ListEmptyComponent={
          <View style={[issuesCardStyles.state, { borderColor: `${colorPalette.secondary}55` }]}>
            {isLoading ? <ActivityIndicator color={colorPalette.primary} /> : null}
            <Text style={[issuesCardStyles.stateTitle, { color: colorPalette.text }]}>{isError ? "Issues unavailable" : "Issues"}</Text>
            <Text style={[issuesCardStyles.stateText, { color: colorPalette.secondary }]}>{stateMessage}</Text>
          </View>
        }
        onRefresh={() => void refetch()}
        refreshing={isRefetching}
        renderItem={renderIssue}
        showsVerticalScrollIndicator={false}
      />
      <AddIssueModal siteId={siteId} visible={isAddModalOpen} onClose={() => setIsAddModalOpen(false)} />
      <IssueDetailsModal issueId={selectedIssueId} onClose={() => setSelectedIssueId(null)} />
    </View>
  );
};

export default IssuesCard;
