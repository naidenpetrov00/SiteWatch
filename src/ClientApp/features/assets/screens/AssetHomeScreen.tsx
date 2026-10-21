import Ionicons from "@expo/vector-icons/Ionicons";
import { useRouter } from "expo-router";
import { Pressable, ScrollView, Text, View } from "react-native";

import assetStyles from "../assets.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const actions = [
  {
    description: "Scan the opaque QR identifier on an active machine or tool.",
    icon: "qr-code-outline" as const,
    label: "Scan QR code",
    route: "/Assets/scan" as const,
  },
  {
    description: "Find an active asset by code, name, manufacturer, model, or serial number.",
    icon: "search-outline" as const,
    label: "Search manually",
    route: "/Assets/search" as const,
  },
  {
    description: "Submit one machine or tool for administrator review.",
    icon: "add-circle-outline" as const,
    label: "Register an asset",
    route: "/Assets/register" as const,
  },
];

export default function AssetHomeScreen() {
  const colors = useColorPalette();
  const router = useRouter();

  return (
    <ScrollView
      contentContainerStyle={assetStyles.content}
      contentInsetAdjustmentBehavior="automatic"
      style={[assetStyles.screen, { backgroundColor: colors.background }]}
    >
      <View style={{ gap: 6 }}>
        <Text style={[assetStyles.title, { color: colors.text }]}>Asset tracking</Text>
        <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
          Discover approved assets or submit a new registration for review.
        </Text>
      </View>
      <View style={assetStyles.actionGrid}>
        {actions.map((action) => (
          <Pressable
            accessibilityRole="button"
            key={action.route}
            onPress={() => router.push(action.route)}
            style={({ pressed }) => [
              assetStyles.actionCard,
              {
                borderColor: colors.primary,
                opacity: pressed ? 0.78 : 1,
              },
            ]}
          >
            <View style={assetStyles.actionHeader}>
              <Ionicons color={colors.primary} name={action.icon} size={24} />
              <Text style={[assetStyles.actionTitle, { color: colors.text }]}>
                {action.label}
              </Text>
            </View>
            <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
              {action.description}
            </Text>
          </Pressable>
        ))}
      </View>
    </ScrollView>
  );
}
