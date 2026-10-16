import { useMemo, useState } from "react";
import { FlatList, Modal, Pressable, RefreshControl, Text, TextInput, View } from "react-native";
import { isAxiosError } from "axios";
import { useQueryClient } from "@tanstack/react-query";

import { useColorPalette } from "@/hooks/useColorPalette";
import { proposalKeys, useClientProposal, useRespondToProposal, useShareProposalPdf } from "../api";
import type { ClientProposalDetails, ProposalResponseDecision } from "../types";
import { formatProposalDateOnly, formatProposalTimestamp } from "../formatters";
import styles from "./ProposalScreens.styles";

const unavailable = "—";
const quantity = (value: number) => new Intl.NumberFormat(undefined, { maximumFractionDigits: 8 }).format(value);
const money = (value: number, currencyCode: string | null | undefined) => {
  if (typeof currencyCode !== "string" || !currencyCode.trim()) {
    return `${quantity(value)} (currency unavailable)`;
  }

  try {
    return new Intl.NumberFormat(undefined, { style: "currency", currency: currencyCode }).format(value);
  } catch {
    return `${quantity(value)} (${currencyCode})`;
  }
};
const optionalText = (value: string | null | undefined) =>
  typeof value === "string" && value.trim() ? value : unavailable;
const optionalQuantity = (value: number | null, unit: string | null) =>
  value === null ? unavailable : `${quantity(value)}${unit ? ` ${unit}` : ""}`;
const optionalMoney = (value: number | null, currencyCode: string | null) =>
  value === null || !currencyCode ? unavailable : money(value, currencyCode);
const isProposalConflict = (error: unknown) =>
  isAxiosError<{ title?: unknown }>(error) &&
  error.response?.status === 409 &&
  error.response.data?.title === "Proposal conflict";
const DetailRow = ({ label, value }: { label: string; value: string }) => {
  const palette = useColorPalette();
  return <View style={styles.row}><Text style={[styles.rowLabel, { color: palette.secondary }]}>{label}</Text><Text selectable style={[styles.rowValue, { color: palette.text }]}>{value}</Text></View>;
};

function ProposalSection({ section, proposal }: { section: string; proposal: ClientProposalDetails }) {
  const palette = useColorPalette();
  const sectionStyle = [styles.section, { borderColor: `${palette.secondary}55` }];
  if (section === "identity") return <View style={sectionStyle}><Text style={[styles.sectionTitle, { color: palette.text }]}>Proposal</Text><DetailRow label="Number" value={`#${proposal.numberId} · Revision ${proposal.revisionNumber}`} /><DetailRow label="Status" value={proposal.status} /><DetailRow label="Issued" value={formatProposalTimestamp(proposal.issuedAt)} /><DetailRow label="Valid until" value={formatProposalDateOnly(proposal.validUntil)} /><DetailRow label="Site" value={`#${proposal.siteNumberId} · ${proposal.siteName}`} /><DetailRow label="Address" value={proposal.siteAddress} /><DetailRow label="Recipient" value={proposal.recipientDisplayName} /><DetailRow label="Email" value={proposal.recipientEmail} /></View>;
  if (section === "interaction") return <View style={sectionStyle}><Text style={[styles.sectionTitle, { color: palette.text }]}>Response</Text><DetailRow label="First viewed" value={proposal.firstViewedAt ? formatProposalTimestamp(proposal.firstViewedAt) : "Not viewed"} /><DetailRow label="Responded" value={proposal.respondedAt ? formatProposalTimestamp(proposal.respondedAt) : "Awaiting response"} /><DetailRow label="Comment" value={proposal.responseComment ?? "No comment provided"} /></View>;
  if (section === "activities") return <View style={sectionStyle}><Text style={[styles.sectionTitle, { color: palette.text }]}>Activities</Text>{proposal.activities.map((activity) => <View key={activity.id} style={styles.item}><Text style={[styles.text, { color: palette.text, fontWeight: "700" }]}>#{activity.activityNumberId} · {optionalText(activity.name)}</Text><DetailRow label="Description" value={optionalText(activity.description)} />{activity.sections.map((item) => <Text key={item.id} style={[styles.muted, { color: palette.secondary }]}>{optionalText(item.name)}: Requested {quantity(item.requestedMeasurement)} {optionalText(item.measurementUnit)} · Basis {quantity(item.basisQuantity)} · {item.pricingMode === "free" ? "Free" : optionalMoney(item.priceAmount, proposal.currencyCode)} · Total {money(item.calculatedTotal, proposal.currencyCode)}</Text>)}</View>)}</View>;
  if (section === "products") return <View style={sectionStyle}><Text style={[styles.sectionTitle, { color: palette.text }]}>Products</Text>{proposal.products.map((product) => <View key={product.id} style={[styles.item, { borderBottomColor: `${palette.secondary}44` }]}><Text style={[styles.text, { color: palette.text, fontWeight: "700" }]}>#{product.productNumberId} · {optionalText(product.title)}</Text><DetailRow label="Category" value={optionalText(product.category)} /><DetailRow label="Brand" value={optionalText(product.brand)} /><DetailRow label="Model" value={optionalText(product.model)} /><DetailRow label="Package" value={optionalQuantity(product.packageQuantity, product.packageUnit)} /><DetailRow label="Selected retailer" value={optionalText(product.selectedRetailerDisplayName)} /><DetailRow label="Selected price" value={product.selectedPriceAmount === null ? "No selected price" : optionalMoney(product.selectedPriceAmount, product.selectedPriceCurrencyCode)} /><DetailRow label="Selected price currency" value={optionalText(product.selectedPriceCurrencyCode)} /><DetailRow label="Selected price basis" value={optionalText(product.selectedPriceBasis)} /><DetailRow label="Required quantity" value={quantity(product.requiredQuantity)} /><DetailRow label="Optional quantity" value={quantity(product.optionalQuantity)} />{product.isOptionalPriceExcluded ? <Text style={styles.warningText}>Optional product — no selected price; it is not included in the proposal total.</Text> : null}<DetailRow label="Required total" value={optionalMoney(product.requiredTotal, proposal.currencyCode)} /><DetailRow label="Optional total" value={optionalMoney(product.optionalTotal, proposal.currencyCode)} /></View>)}</View>;
  if (section === "terms") return <View style={sectionStyle}><Text style={[styles.sectionTitle, { color: palette.text }]}>Notes &amp; payment</Text><DetailRow label="Public notes" value={proposal.publicNotes ?? "—"} /><DetailRow label="Payment terms" value={proposal.paymentTerms ?? "—"} /></View>;
  return <View style={sectionStyle}><Text style={[styles.sectionTitle, { color: palette.text }]}>Commercial summary</Text><DetailRow label="Activities subtotal" value={money(proposal.activitySubtotalBeforeDiscount, proposal.currencyCode)} /><DetailRow label={`Activity discount (${proposal.activityDiscountPercentage}%)`} value={`−${money(proposal.activityDiscountAmount, proposal.currencyCode)}`} /><DetailRow label="Activities total" value={money(proposal.activityTotalAfterDiscount, proposal.currencyCode)} /><DetailRow label="Products subtotal" value={money(proposal.productSubtotalBeforeDiscount, proposal.currencyCode)} /><DetailRow label={`Product discount (${proposal.productDiscountPercentage}%)`} value={`−${money(proposal.productDiscountAmount, proposal.currencyCode)}`} /><DetailRow label="Products total" value={money(proposal.productTotalAfterDiscount, proposal.currencyCode)} /><DetailRow label="Proposal total" value={money(proposal.total, proposal.currencyCode)} /></View>;
}

export default function ProposalDetailScreen({ siteId, proposalId }: { siteId?: string; proposalId?: string }) {
  const palette = useColorPalette();
  const query = useClientProposal(siteId, proposalId);
  const response = useRespondToProposal();
  const share = useShareProposalPdf();
  const queryClient = useQueryClient();
  const [decision, setDecision] = useState<ProposalResponseDecision | null>(null);
  const [comment, setComment] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const sections = useMemo(() => ["identity", "interaction", "activities", "products", "terms", "totals"], []);
  const proposal = query.data;

  const confirm = async () => {
    if (!decision || !siteId || !proposalId) return;
    setMessage(null);
    try {
      await response.mutateAsync({ siteId, proposalId, decision, comment: comment.trim() || null });
      setDecision(null);
      setComment("");
    } catch (error) {
      setDecision(null);
      if (isProposalConflict(error)) {
        await Promise.all([
          queryClient.invalidateQueries({ queryKey: proposalKeys.site(siteId), refetchType: "all" }),
          queryClient.invalidateQueries({ queryKey: proposalKeys.detail(siteId, proposalId), refetchType: "all" }),
        ]);
        setMessage("This Proposal was already answered or a newer revision changed whether it can be answered. The latest server state has been loaded.");
      } else {
        await query.refetch();
        setMessage("The Proposal state changed or your response could not be saved. The latest state has been loaded.");
      }
    }
  };

  if (query.isPending) return <View style={styles.centered}><Text style={{ color: palette.text }}>Loading Proposal…</Text></View>;
  if (query.isError || !proposal || !siteId || !proposalId) return <View style={styles.centered}><Text style={styles.error}>This Proposal is unavailable or your Site access has changed.</Text><Pressable style={[styles.button, { backgroundColor: palette.primary }]} onPress={() => query.refetch()}><Text style={{ color: palette.contrastText }}>Try again</Text></Pressable></View>;

  return <>
    <FlatList
      style={[styles.screen, { backgroundColor: palette.background }]}
      contentContainerStyle={styles.content}
      data={sections}
      keyExtractor={(item) => item}
      renderItem={({ item }) => <ProposalSection section={item} proposal={proposal} />}
      refreshControl={<RefreshControl refreshing={query.isRefetching} onRefresh={query.refetch} tintColor={palette.primary} />}
      ListHeaderComponent={<View style={{ gap: 12 }}><Text style={[styles.title, { color: palette.text }]}>Proposal #{proposal.numberId} · Revision {proposal.revisionNumber}</Text>{proposal.excludesUnpricedOptionalItems ? <View style={styles.warning}><Text style={styles.warningText}>The total excludes {proposal.unpricedOptionalItemCount} unpriced optional item{proposal.unpricedOptionalItemCount === 1 ? "" : "s"}.</Text></View> : null}{proposal.supersededByRevisionNumber ? <View style={styles.warning}><Text style={styles.warningText}>Superseded by revision {proposal.supersededByRevisionNumber}. This revision remains available for reference.</Text></View> : null}{message ? <Text style={styles.error}>{message}</Text> : null}<View style={styles.actions}>{proposal.canRespond ? <><Pressable style={[styles.button, { backgroundColor: palette.primary }]} onPress={() => setDecision("Accepted")}><Text style={[styles.buttonText, { color: palette.contrastText }]}>Accept</Text></Pressable><Pressable style={[styles.button, styles.dangerButton]} onPress={() => setDecision("Rejected")}><Text style={[styles.buttonText, { color: "#fff" }]}>Reject</Text></Pressable></> : null}{proposal.hasPdf ? <Pressable disabled={share.isPending} style={[styles.button, styles.secondaryButton, { borderColor: palette.primary }]} onPress={() => share.mutate({ siteId, proposalId }, { onError: () => setMessage("The PDF could not be downloaded or shared.") })}><Text style={[styles.buttonText, { color: palette.primary }]}>{share.isPending ? "Preparing…" : "Share / Save PDF"}</Text></Pressable> : null}</View></View>}
    />
    <Modal visible={decision !== null} animationType="slide" presentationStyle="formSheet" onRequestClose={() => setDecision(null)}>
      <View style={[styles.modal, { backgroundColor: palette.background }]}><Text style={[styles.modalTitle, { color: palette.text }]}>{decision} Proposal?</Text><Text style={[styles.text, { color: palette.text }]}>This response is final and cannot be changed.</Text><TextInput multiline maxLength={2000} placeholder="Optional comment" placeholderTextColor={palette.placeholderText} value={comment} onChangeText={setComment} style={[styles.input, { color: palette.text, borderColor: palette.secondary }]} /><Text style={[styles.counter, { color: palette.secondary }]}>{comment.length}/2000</Text><View style={styles.actions}><Pressable style={[styles.button, styles.secondaryButton, { borderColor: palette.secondary }]} onPress={() => setDecision(null)}><Text style={{ color: palette.text }}>Cancel</Text></Pressable><Pressable disabled={response.isPending} style={[styles.button, { backgroundColor: decision === "Rejected" ? "#b91c1c" : palette.primary }]} onPress={confirm}><Text style={[styles.buttonText, { color: "#fff" }]}>{response.isPending ? "Saving…" : `Confirm ${decision}`}</Text></Pressable></View></View>
    </Modal>
  </>;
}
