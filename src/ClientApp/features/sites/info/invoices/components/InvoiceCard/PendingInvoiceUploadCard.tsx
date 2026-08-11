import { ActivityIndicator, Text, View } from "react-native";
import Animated, { FadeInDown } from "react-native-reanimated";

import { useColorPalette } from "@/hooks/useColorPalette";
import invoiceCardStyles from "./InvoiceCard.styles";

type PendingInvoiceUploadCardProps = {
  fileName: string;
};

const PendingInvoiceUploadCard = ({ fileName }: PendingInvoiceUploadCardProps) => {
  const colorPalette = useColorPalette();

  return (
    <Animated.View
      entering={FadeInDown.duration(180)}
      accessibilityLabel={`${fileName} uploading`}
      style={[
        invoiceCardStyles.card,
        {
          backgroundColor: `${colorPalette.secondary}22`,
          borderColor: `${colorPalette.secondary}88`,
          gap: 8,
        },
      ]}
    >
      <ActivityIndicator color={colorPalette.primary} />
      <Text style={[invoiceCardStyles.invoiceNumber, { color: colorPalette.text }]}>
        {fileName}
      </Text>
      <Text style={[invoiceCardStyles.metadata, { color: colorPalette.secondary }]}>
        Uploading invoice…
      </Text>
    </Animated.View>
  );
};

export default PendingInvoiceUploadCard;
