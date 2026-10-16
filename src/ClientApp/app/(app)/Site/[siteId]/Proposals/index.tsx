import ProposalListScreen from "@/features/sites/proposals/components/ProposalListScreen";
import useGetSearchParams from "@/hooks/useGetSearchParams";

export default function ProposalsRoute() {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  return <ProposalListScreen siteId={siteId} />;
}
