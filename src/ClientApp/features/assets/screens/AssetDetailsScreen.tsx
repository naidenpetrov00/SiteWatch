import { ActivityIndicator, ScrollView, Text, View } from "react-native";

import AssetDetailRows from "../components/AssetDetailRows";
import AssetErrorState from "../components/AssetErrorState";
import { getAssetApiError, useMobileAsset } from "../api";
import assetStyles from "../assets.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function AssetDetailsScreen({ assetId }: { assetId: string }) {
  const colors = useColorPalette();
  const query = useMobileAsset(assetId);

  if (query.isLoading) {
    return <View style={[assetStyles.centerState, { backgroundColor: colors.background }]}><ActivityIndicator color={colors.primary} size="large" /></View>;
  }
  if (query.isError || !query.data) {
    return (
      <View style={[assetStyles.screen, { backgroundColor: colors.background }]}>
        <AssetErrorState
          message={getAssetApiError(query.error, "This asset could not be loaded.")}
          onRetry={() => void query.refetch()}
        />
      </View>
    );
  }

  const asset = query.data;
  return (
    <ScrollView
      contentContainerStyle={assetStyles.content}
      contentInsetAdjustmentBehavior="automatic"
      style={[assetStyles.screen, { backgroundColor: colors.background }]}
    >
      <View style={{ gap: 4 }}>
        <Text style={[assetStyles.title, { color: colors.text }]}>{asset.name}</Text>
        <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
          {asset.assetCode} · {asset.classification}
        </Text>
      </View>
      <View style={[assetStyles.section, { borderColor: colors.secondary }]}>
        <AssetDetailRows details={[
          { label: "Site", value: asset.siteName },
          { label: "Manufacturer", value: asset.manufacturer },
          { label: "Model", value: asset.model },
          { label: "Serial number", value: asset.manufacturerSerialNumber },
          { label: "Condition", value: asset.condition },
          { label: "Operational status", value: asset.operationalStatus },
          { label: "Responsible assignee", value: asset.responsibleAssigneeDisplayName },
          { label: "Description", value: asset.description },
          { label: "QR identifier", value: asset.qrIdentifier },
        ]} />
      </View>
    </ScrollView>
  );
}
