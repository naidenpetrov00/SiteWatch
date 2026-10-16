import { Stack } from "expo-router";

export default function ProposalsLayout() {
  return <Stack><Stack.Screen name="index" options={{ title: "Proposals" }} /><Stack.Screen name="[proposalId]" options={{ title: "Proposal details" }} /></Stack>;
}
