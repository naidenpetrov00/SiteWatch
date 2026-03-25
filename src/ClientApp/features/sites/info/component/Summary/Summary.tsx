import { Text, View } from "react-native";

import summaryStyles from "./Summary.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const Summary = () => {
  const colorPalette = useColorPalette();
  const siteSummary = {
    name: "North Gate Logistics Hub",
    address: "1420 Industrial Park Rd, Building C, Sofia",
    status: "Operational",
    manager: "Elena Petrova",
    camerasOnline: "18 / 20",
    schedule: "03 Mar 2026 - 18 Apr 2026",
    currentDuration: "3 weeks, 1 day",
  };

  const details = [
    { label: "Status", value: siteSummary.status },
    { label: "Site Manager", value: siteSummary.manager },
    { label: "Cameras Online", value: siteSummary.camerasOnline },
    { label: "Start / End Date", value: siteSummary.schedule },
    { label: "Current Duration", value: siteSummary.currentDuration },
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
          {siteSummary.name}
        </Text>
        <Text
          style={[summaryStyles.address, { color: colorPalette.secondary }]}
        >
          {siteSummary.address}
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
            {siteSummary.status}
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
