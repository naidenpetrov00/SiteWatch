import { Text, View } from "react-native";
import DetailCard from "../../ui/DetailCard/DetailCard";
import detailsStyles from "./Details.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const Details = () => {
  const colorPalette = useColorPalette();
  const detailCards = [
    { label: "Zones Covered", value: "4", helper: "North, South, East, West" },
    { label: "People On Site", value: "27", helper: "11 contractors active" },
    { label: "Open Issues", value: "2", helper: "1 camera, 1 gate sensor" },
    { label: "Network Uptime", value: "99.2%", helper: "Last 30 days" },
  ];

  return (
    <View style={detailsStyles.container}>
      <View style={detailsStyles.grid}>
        {detailCards.map((card) => (
          <DetailCard key={card.label} style={detailsStyles.gridItem}>
            <Text style={[detailsStyles.label, { color: colorPalette.secondary }]}>
              {card.label}
            </Text>
            <Text style={[detailsStyles.value, { color: colorPalette.text }]}>
              {card.value}
            </Text>
            <Text style={[detailsStyles.helper, { color: colorPalette.secondary }]}>
              {card.helper}
            </Text>
          </DetailCard>
        ))}
      </View>
    </View>
  );
};

export default Details;
