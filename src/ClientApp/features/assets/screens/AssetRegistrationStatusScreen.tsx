import { useRouter } from "expo-router";
import { ActivityIndicator, Pressable, ScrollView, Text, View } from "react-native";

import { getAssetApiError, useMobileAssetRegistration } from "../api";
import assetStyles from "../assets.styles";
import AssetDetailRows from "../components/AssetDetailRows";
import AssetErrorState from "../components/AssetErrorState";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function AssetRegistrationStatusScreen({ assetId }: { assetId: string }) {
  const colors = useColorPalette();
  const router = useRouter();
  const query = useMobileAssetRegistration(assetId);

  if (query.isLoading) {
    return (
      <View style={[assetStyles.centerState, { backgroundColor: colors.background }]}>
        <ActivityIndicator color={colors.primary} size="large" />
      </View>
    );
  }

  if (query.isError || !query.data) {
    return (
      <View style={[assetStyles.screen, { backgroundColor: colors.background }]}>
        <AssetErrorState
          message={getAssetApiError(query.error, "This registration could not be loaded.")}
          onRetry={() => void query.refetch()}
        />
      </View>
    );
  }

  const registration = query.data;
  const statusMessage = registration.reviewStatus === "PendingReview"
    ? "Pending administrator review. This asset is not active yet."
    : registration.reviewStatus === "Active"
      ? "Approved and active. You can now open the asset details."
      : "The registration was not approved. Contact an administrator if you need more information.";

  return (
    <ScrollView
      contentContainerStyle={assetStyles.content}
      contentInsetAdjustmentBehavior="automatic"
      style={[assetStyles.screen, { backgroundColor: colors.background }]}
    >
      <View style={{ gap: 6 }}>
        <Text style={[assetStyles.title, { color: colors.text }]}>{registration.name}</Text>
        <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
          {registration.assetCode} · {registration.classification}
        </Text>
      </View>
      <View style={[assetStyles.statusBadge, { backgroundColor: `${colors.primary}22` }]}>
        <Text style={[assetStyles.statusText, { color: colors.primary }]}>
          {registration.reviewStatus === "PendingReview" ? "Pending review" : registration.reviewStatus}
        </Text>
      </View>
      <Text accessibilityRole="summary" style={[assetStyles.subtitle, { color: colors.text }]}>{statusMessage}</Text>
      <View style={[assetStyles.section, { borderColor: colors.secondary }]}>
        <AssetDetailRows details={[
          { label: "Site", value: registration.siteName },
          { label: "Manufacturer", value: registration.manufacturer },
          { label: "Model", value: registration.model },
          { label: "Serial number", value: registration.manufacturerSerialNumber },
          { label: "Responsibility", value: registration.isAssignedToCurrentUser ? "Assigned to me" : "Unassigned" },
          { label: "Condition", value: registration.condition },
          { label: "Operational status", value: registration.operationalStatus },
          { label: "Description", value: registration.description },
          { label: "QR identifier", value: registration.qrIdentifier },
        ]} />
      </View>
      <View style={assetStyles.buttonRow}>
        {registration.reviewStatus === "Active" ? (
          <Pressable
            accessibilityRole="button"
            onPress={() => router.replace({ pathname: "/Assets/[id]", params: { id: registration.id } })}
            style={({ pressed }) => [
              assetStyles.button,
              { backgroundColor: colors.primary, opacity: pressed ? 0.78 : 1 },
            ]}
          >
            <Text style={[assetStyles.buttonText, { color: colors.background }]}>Open asset</Text>
          </Pressable>
        ) : null}
        <Pressable
          accessibilityRole="button"
          disabled={query.isFetching}
          onPress={() => void query.refetch()}
          style={({ pressed }) => [
            assetStyles.button,
            {
              borderColor: colors.secondary,
              borderWidth: 1,
              opacity: query.isFetching ? 0.5 : pressed ? 0.78 : 1,
            },
          ]}
        >
          <Text style={[assetStyles.buttonText, { color: colors.text }]}>
            {query.isFetching ? "Refreshing…" : "Refresh status"}
          </Text>
        </Pressable>
      </View>
    </ScrollView>
  );
}
