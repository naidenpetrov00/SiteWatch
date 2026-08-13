import { Text, View } from "react-native";

import summaryStyles from "./Summary.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { Site } from "@/features/sites/api/types";
import { formatSiteDate, formatSiteDuration } from "../site-info-formatters";

type SummaryProps = {
  site: Site;
};

const Summary = ({ site }: SummaryProps) => {
  const colorPalette = useColorPalette();

  const details = [
    { label: "Status", value: site.status },
    { label: "Site Manager", value: site.managerDisplayName },
    { label: "Start / End Date", value: `${formatSiteDate(site.startDate)} - ${formatSiteDate(site.endDate)}` },
    { label: "Current Duration", value: formatSiteDuration(site.startDate, site.endDate) },
  ];

  return (
    <View style={summaryStyles.container}>
      <View
        style={[
          summaryStyles.card,
          {
            backgroundColor: colorPalette.background,
            borderColor: colorPalette.primary,
          },
        ]}
      >
        <Text style={[summaryStyles.title, { color: colorPalette.text }]}>
          {site.name}
        </Text>
        <Text
          style={[summaryStyles.address, { color: colorPalette.secondary }]}
        >
          {site.address}
        </Text>

        <View
          style={[
            summaryStyles.statusBadge,
            { backgroundColor: colorPalette.primary },
          ]}
        >
          <Text
            style={[
              summaryStyles.statusBadgeText,
              { color: colorPalette.background },
            ]}
          >
            {site.status}
          </Text>
        </View>

        <View style={summaryStyles.detailsList}>
          {details.map((detail) => (
            <View key={detail.label} style={summaryStyles.detailRow}>
              <Text
                style={[
                  summaryStyles.detailLabel,
                  { color: colorPalette.secondary },
                ]}
              >
                {detail.label}
              </Text>
              <Text
                style={[
                  summaryStyles.detailValue,
                  { color: colorPalette.text },
                ]}
              >
                {detail.value}
              </Text>
            </View>
          ))}
        </View>
      </View>
    </View>
  );
};

export default Summary;
