import { FlatList, Text, View } from "react-native";

import DetailCard from "../../ui/DetailCard/DetailCard";
import { DetailsCardItem } from "../../types";
import detailsStyles from "./Details.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { ACCESS_POLICIES } from "@/types/authorization";
import { useAuth } from "@/store/auth_context";

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
    value: "Unavailable",
    helper: "Not available yet",
  },
  {
    label: "Open Issues",
    value: "Unavailable",
    helper: "Not available yet",
  },
  { label: "Payments", value: "Unavailable", helper: "Not available yet" },
];

const Details = () => {
  const localParams = useGetSearchParams<{ siteId?: string }>();
  const siteId = localParams.siteId;
  const colorPalette = useColorPalette();
  const { hasAnyRole } = useAuth();
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
                {card.value}
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
