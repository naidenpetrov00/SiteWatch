import { Pressable, Text, View } from "react-native";

import assetStyles from "../assets.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

type Props = {
  message: string;
  onRetry?: () => void;
};

export default function AssetErrorState({ message, onRetry }: Props) {
  const colors = useColorPalette();

  return (
    <View style={assetStyles.centerState}>
      <Text accessibilityRole="alert" style={[assetStyles.centerText, { color: colors.text }]}>
        {message}
      </Text>
      {onRetry ? (
        <Pressable
          accessibilityRole="button"
          onPress={onRetry}
          style={({ pressed }) => [
            assetStyles.button,
            { backgroundColor: colors.primary, opacity: pressed ? 0.78 : 1 },
          ]}
        >
          <Text style={[assetStyles.buttonText, { color: colors.background }]}>Try again</Text>
        </Pressable>
      ) : null}
    </View>
  );
}
