import { FlatList, Text, View } from "react-native";
import { useGlobalSearchParams, useLocalSearchParams } from "expo-router";

import DetailCard from "../../ui/DetailCard/DetailCard";
import { DetailsCardItem } from "../../types";
import detailsStyles from "./Details.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const detailCards: DetailsCardItem[] = [
  { label: "Images", value: "24", helper: "Galery", path: "Images" },
  {
    label: "People On Site",
    value: "27",
    helper: "11 contractors active",
  },
  {
    label: "Open Issues",
    value: "2",
    helper: "1 camera, 1 gate sensor",
  },
  { label: "Payments", value: "2000", helper: "All" },
];

const Details = () => {
  const localParams = useLocalSearchParams<{ siteId?: string }>();
  const siteId = useGlobalSearchParams(siteId);
  const colorPalette = useColorPalette();

  return (
    <View style={detailsStyles.container}>
      <FlatList
        data={detailCards}
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
