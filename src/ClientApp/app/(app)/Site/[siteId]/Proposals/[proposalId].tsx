import ProposalDetailScreen from "@/features/sites/proposals/components/ProposalDetailScreen";
import useGetSearchParams from "@/hooks/useGetSearchParams";

export default function ProposalDetailsRoute() {
  const { siteId, proposalId } = useGetSearchParams<{ siteId?: string; proposalId?: string }>();
  return <ProposalDetailScreen siteId={siteId} proposalId={proposalId} />;
}
