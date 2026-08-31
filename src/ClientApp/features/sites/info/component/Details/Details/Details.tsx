import { FlatList, Text, View } from "react-native";

import DetailCard from "../../ui/DetailCard/DetailCard";
import { DetailsCardItem } from "../../types";
import detailsStyles from "./Details.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { ACCESS_POLICIES } from "@/types/authorization";
import { useAuth } from "@/store/auth_context";
import { useGetSiteFileIdsBySiteId } from "@/features/sites/info/files/hooks/useGetSiteFileIdsBySiteId";
import { useGetSiteImageIdsBySiteId } from "@/features/sites/info/images/hooks/useGetSiteImageIdsBySiteId";
import { useGetSiteInvoices } from "@/features/sites/info/invoices/hooks/useGetSiteInvoices";
import { useGetSiteVideoIdsBySiteId } from "@/features/sites/info/videos/hooks/useGetSiteVideoIdsBySiteId";

const temporaryCount = (minimum: number, maximum: number) =>
  String(Math.floor(Math.random() * (maximum - minimum + 1)) + minimum);

const detailCards: DetailsCardItem[] = [
  { label: "Images", value: "View", helper: "Gallery", path: "Images" },
  { label: "Videos", value: "View", helper: "Gallery", path: "Videos" },
  {
    label: "Invoices",
    value: "View",
    helper: "Billing documents",
    path: "Invoices",
    allowedRoles: ACCESS_POLICIES.siteInvoices,
  },
  { label: "Files", value: "View", helper: "Files", path: "Files" },
  {
    label: "People On Site",
    value: temporaryCount(1, 50),
    helper: "Temporary data",
    path: "People",
  },
  {
    label: "Payments",
    value: temporaryCount(0, 25),
    helper: "Temporary data",
    path: "Payments",
  },
];

const Details = () => {
  const localParams = useGetSearchParams<{ siteId?: string }>();
  const siteId = localParams.siteId;
  const colorPalette = useColorPalette();
  const { hasAnyRole } = useAuth();
  const canViewInvoices = hasAnyRole(ACCESS_POLICIES.siteInvoices);
  const imageIdsQuery = useGetSiteImageIdsBySiteId({ siteId });
  const videoIdsQuery = useGetSiteVideoIdsBySiteId({ siteId });
  const fileIdsQuery = useGetSiteFileIdsBySiteId({ siteId });
  const invoicesQuery = useGetSiteInvoices({
    siteId,
    enabled: canViewInvoices,
  });
  const resourceCounts: Record<string, string> = {
    Images: imageIdsQuery.isSuccess ? String(imageIdsQuery.data.length) : "—",
    Videos: videoIdsQuery.isSuccess ? String(videoIdsQuery.data.length) : "—",
    Invoices: invoicesQuery.isSuccess ? String(invoicesQuery.data.length) : "—",
    Files: fileIdsQuery.isSuccess ? String(fileIdsQuery.data.length) : "—",
  };
  const visibleDetailCards = detailCards.filter(
    (card) => !card.allowedRoles || hasAnyRole(card.allowedRoles),
  );

  return (
    <View style={detailsStyles.container}>
      <FlatList<DetailsCardItem>
        data={visibleDetailCards}
        keyExtractor={(card) => card.label}
        numColumns={2}
        scrollEnabled={false}
        columnWrapperStyle={detailsStyles.row}
        contentContainerStyle={detailsStyles.grid}
        renderItem={({ item: card }) => {
          return (
            <DetailCard
              path={card.path}
              siteId={siteId}
              style={detailsStyles.card}
            >
              <Text
                style={[detailsStyles.label, { color: colorPalette.secondary }]}
              >
                {card.label}
              </Text>
              <Text style={[detailsStyles.value, { color: colorPalette.text }]}>
                {resourceCounts[card.label] ?? card.value}
              </Text>
              <Text
                style={[
                  detailsStyles.helper,
                  { color: colorPalette.secondary },
                ]}
              >
                {card.helper}
              </Text>
            </DetailCard>
          );
        }}
      />
    </View>
  );
};

export default Details;
