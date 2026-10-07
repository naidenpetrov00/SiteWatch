import { memo, useCallback } from "react";
import { FlatList, Pressable, RefreshControl, Text, View } from "react-native";
import { useRouter } from "expo-router";

import { useColorPalette } from "@/hooks/useColorPalette";
import { useClientProposals } from "../api";
import type { ClientProposalSummary } from "../types";
import { formatProposalDateOnly, formatProposalTimestamp } from "../formatters";
import styles from "./ProposalScreens.styles";

const money = (value: number) =>
  new Intl.NumberFormat(undefined, { style: "currency", currency: "EUR" }).format(value);
const ProposalCard = memo(function ProposalCard({
  id,
  siteId,
  numberId,
  revisionNumber,
  status,
  issuedAt,
  validUntil,
  total,
  canRespond,
  supersededByRevisionNumber,
}: {
  id: string;
  siteId: string;
  numberId: number;
  revisionNumber: number;
  status: string;
  issuedAt: string;
  validUntil: string | null;
  total: number;
  canRespond: boolean;
  supersededByRevisionNumber: number | null;
}) {
  const palette = useColorPalette();
  const router = useRouter();
  const open = useCallback(() => {
    router.push({
      pathname: "/Site/[siteId]/Proposals/[proposalId]",
      params: { siteId, proposalId: id },
    });
  }, [id, router, siteId]);

  return (
    <Pressable
      accessibilityRole="button"
      onPress={open}
      style={({ pressed }) => [
        styles.card,
        { borderColor: `${palette.primary}66`, opacity: pressed ? 0.75 : 1 },
      ]}
    >
      <View style={styles.cardHeader}>
        <Text style={[styles.title, { color: palette.text }]}>Proposal #{numberId} · Rev {revisionNumber}</Text>
        <Text style={[styles.status, { color: palette.primary }]}>{status}</Text>
      </View>
      <Text style={[styles.text, { color: palette.text }]}>{money(total)}</Text>
      <Text style={[styles.muted, { color: palette.secondary }]}>Issued {formatProposalTimestamp(issuedAt)} · Valid until {formatProposalDateOnly(validUntil)}</Text>
      {canRespond ? <Text style={[styles.muted, { color: palette.primary }]}>Awaiting your response</Text> : null}
      {supersededByRevisionNumber ? <Text style={[styles.muted, { color: palette.secondary }]}>Superseded by revision {supersededByRevisionNumber}</Text> : null}
    </Pressable>
  );
});

export default function ProposalListScreen({ siteId }: { siteId?: string }) {
  const palette = useColorPalette();
  const query = useClientProposals(siteId);
  const renderItem = useCallback(
    ({ item }: { item: ClientProposalSummary }) => (
      <ProposalCard
        id={item.id}
        siteId={siteId!}
        numberId={item.numberId}
        revisionNumber={item.revisionNumber}
        status={item.status}
        issuedAt={item.issuedAt}
        validUntil={item.validUntil}
        total={item.total}
        canRespond={item.canRespond}
        supersededByRevisionNumber={item.supersededByRevisionNumber}
      />
    ),
    [siteId],
  );

  if (query.isPending) {
    return <View style={styles.centered}><Text style={{ color: palette.text }}>Loading Proposals…</Text></View>;
  }
  if (query.isError || !siteId) {
    return <View style={styles.centered}><Text style={styles.error}>Proposals could not be loaded. Access may have changed.</Text><Pressable style={[styles.button, { backgroundColor: palette.primary }]} onPress={() => query.refetch()}><Text style={{ color: palette.contrastText }}>Try again</Text></Pressable></View>;
  }

  return (
    <FlatList
      style={[styles.screen, { backgroundColor: palette.background }]}
      contentContainerStyle={query.data.length ? styles.content : styles.centered}
      data={query.data}
      keyExtractor={(item) => item.id}
      renderItem={renderItem}
      ListEmptyComponent={<Text style={[styles.text, { color: palette.secondary }]}>No issued Proposals are addressed to you for this Site.</Text>}
      refreshControl={<RefreshControl refreshing={query.isRefetching} onRefresh={query.refetch} tintColor={palette.primary} />}
    />
  );
}
