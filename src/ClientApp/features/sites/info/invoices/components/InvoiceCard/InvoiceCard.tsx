import Ionicons from "@expo/vector-icons/Ionicons";
import { memo } from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import { formatInvoiceAmount, formatInvoiceDate } from "../../formatters";
import invoiceCardStyles from "./InvoiceCard.styles";

type InvoiceCardProps = {
  id: string;
  numberId: number;
  invoiceNumber: string | null;
  supplierDisplayLabel: string | null;
  submittedFromSiteName: string | null;
  date: string | null;
  created: string;
  isComplete: boolean;
  totalValueIncludingVat: number | null;
  allocatedAmount: number | null;
  isFileActionDisabled: boolean;
  isOpeningFile: boolean;
  onSelect: (invoiceId: string) => void;
  onOpenFile: (invoiceId: string) => void;
};

const InvoiceCard = memo(function InvoiceCard({
  id,
  numberId,
  invoiceNumber,
  supplierDisplayLabel,
  submittedFromSiteName,
  date,
  created,
  isComplete,
  totalValueIncludingVat,
  allocatedAmount,
  isFileActionDisabled,
  isOpeningFile,
  onSelect,
  onOpenFile,
}: InvoiceCardProps) {
  const colorPalette = useColorPalette();
  const displayName = invoiceNumber ?? `Invoice #${numberId}`;

  return (
    <View
      style={[
        invoiceCardStyles.card,
        {
          backgroundColor:
            isComplete !== true ? "#fee2e2" : colorPalette.background,
          borderColor: `${colorPalette.secondary}55`,
        },
      ]}
    >
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`View ${displayName}`}
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
          {displayName}
        </Text>
        <Text
          style={[invoiceCardStyles.supplier, { color: colorPalette.text }]}
        >
          {supplierDisplayLabel ?? "—"}
        </Text>
        {submittedFromSiteName ? (
          <Text
            style={[invoiceCardStyles.metadata, { color: colorPalette.secondary }]}
          >
            Uploaded from {submittedFromSiteName}
          </Text>
        ) : null}
        <Text
          style={[
            invoiceCardStyles.metadata,
            { color: colorPalette.secondary },
          ]}
        >
          {formatInvoiceDate(date ?? created)} · Total{" "}
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
        accessibilityLabel={`Open file for ${displayName}`}
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
