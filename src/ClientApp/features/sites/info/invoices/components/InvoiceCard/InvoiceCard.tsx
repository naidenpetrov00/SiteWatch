import Ionicons from "@expo/vector-icons/Ionicons";
import { memo } from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import { formatInvoiceAmount, formatInvoiceDate } from "../../formatters";
import invoiceCardStyles from "./InvoiceCard.styles";

type InvoiceCardProps = {
  id: string;
  invoiceNumber: string;
  supplierDisplayLabel: string;
  date: string;
  totalValueIncludingVat: number;
  allocatedAmount: number;
  isFileActionDisabled: boolean;
  isOpeningFile: boolean;
  onSelect: (invoiceId: string) => void;
  onOpenFile: (invoiceId: string) => void;
};

const InvoiceCard = memo(function InvoiceCard({
  id,
  invoiceNumber,
  supplierDisplayLabel,
  date,
  totalValueIncludingVat,
  allocatedAmount,
  isFileActionDisabled,
  isOpeningFile,
  onSelect,
  onOpenFile,
}: InvoiceCardProps) {
  const colorPalette = useColorPalette();

  return (
    <View
      style={[
        invoiceCardStyles.card,
        {
          backgroundColor: colorPalette.background,
          borderColor: `${colorPalette.secondary}55`,
        },
      ]}
    >
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`View invoice ${invoiceNumber}`}
        onPress={() => onSelect(id)}
        style={({ pressed }) => [
          invoiceCardStyles.body,
          pressed ? invoiceCardStyles.pressed : null,
        ]}
      >
        <Text
          style={[
            invoiceCardStyles.invoiceNumber,
            { color: colorPalette.text },
          ]}
        >
          {invoiceNumber}
        </Text>
        <Text
          style={[invoiceCardStyles.supplier, { color: colorPalette.text }]}
        >
          {supplierDisplayLabel}
        </Text>
        <Text
          style={[
            invoiceCardStyles.metadata,
            { color: colorPalette.secondary },
          ]}
        >
          {formatInvoiceDate(date)} · Total{" "}
          {formatInvoiceAmount(totalValueIncludingVat)}
        </Text>
        <Text
          style={[
            invoiceCardStyles.metadata,
            { color: colorPalette.secondary },
          ]}
        >
          Site allocation {formatInvoiceAmount(allocatedAmount)}
        </Text>
      </Pressable>

      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`Open file for invoice ${invoiceNumber}`}
        disabled={isFileActionDisabled}
        onPress={() => onOpenFile(id)}
        style={({ pressed }) => [
          invoiceCardStyles.fileButton,
          { borderLeftColor: `${colorPalette.secondary}55` },
          pressed ? invoiceCardStyles.pressed : null,
        ]}
      >
        {isOpeningFile ? (
          <ActivityIndicator color={colorPalette.primary} />
        ) : (
          <Ionicons
            color={colorPalette.primary}
            name="document-text-outline"
            size={26}
          />
        )}
      </Pressable>
    </View>
  );
});

export default InvoiceCard;
