import { Text, View } from "react-native";

import assetStyles from "../assets.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

type Detail = { label: string; value: string | null | undefined };

export default function AssetDetailRows({ details }: { details: readonly Detail[] }) {
  const colors = useColorPalette();

  return (
    <View style={assetStyles.detailsGrid}>
      {details.map(({ label, value }) => (
        <View key={label} style={assetStyles.detailRow}>
          <Text style={[assetStyles.detailLabel, { color: colors.secondary }]}>{label}</Text>
          <Text selectable style={[assetStyles.detailValue, { color: colors.text }]}>
            {value || "—"}
          </Text>
        </View>
      ))}
    </View>
  );
}
