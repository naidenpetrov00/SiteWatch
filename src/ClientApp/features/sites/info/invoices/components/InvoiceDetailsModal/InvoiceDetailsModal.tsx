import Ionicons from "@expo/vector-icons/Ionicons";
import { Modal, Pressable, ScrollView, Text, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useColorPalette } from "@/hooks/useColorPalette";
import {
  formatInvoiceAmount,
  formatInvoiceDate,
  formatInvoiceDateTime,
  formatInvoiceText,
} from "../../formatters";
import type { SiteInvoice } from "../../types";
import invoiceDetailsModalStyles from "./InvoiceDetailsModal.styles";

type InvoiceDetailsModalProps = {
  invoice: SiteInvoice | null;
  onClose: () => void;
};

type DetailItem = {
  label: string;
  value: string;
};

type DetailSection = {
  title: string;
  items: readonly DetailItem[];
};

const InvoiceDetailsModal = ({
  invoice,
  onClose,
}: InvoiceDetailsModalProps) => {
  const colorPalette = useColorPalette();

  if (!invoice) return null;

  const sections: readonly DetailSection[] = [
    {
      title: "Invoice",
      items: [
        { label: "Invoice number", value: formatInvoiceText(invoice.invoiceNumber) },
        { label: "Reference", value: String(invoice.numberId) },
        { label: "Date", value: formatInvoiceDate(invoice.date) },
        { label: "Uploaded from", value: formatInvoiceText(invoice.submittedFromSiteName) },
        { label: "Supplier", value: formatInvoiceText(invoice.supplierDisplayLabel) },
        { label: "Tax identifier", value: formatInvoiceText(invoice.taxIdentifier) },
        { label: "Address", value: formatInvoiceText(invoice.address) },
        { label: "Contact person", value: formatInvoiceText(invoice.contactPerson) },
        { label: "Email", value: formatInvoiceText(invoice.email) },
        { label: "Phone", value: formatInvoiceText(invoice.phoneNumber) },
      ],
    },
    {
      title: "Amounts",
      items: [
        {
          label: "Excluding VAT",
          value: formatInvoiceAmount(invoice.totalValueExcludingVat),
        },
        { label: "VAT", value: formatInvoiceAmount(invoice.vat) },
        {
          label: "Including VAT",
          value: formatInvoiceAmount(invoice.totalValueIncludingVat),
        },
        {
          label: "Site allocation",
          value: formatInvoiceAmount(invoice.siteAllocation?.amount ?? null),
        },
        { label: "Direction", value: invoice.siteAllocation?.direction ?? "—" },
      ],
    },
    {
      title: "Payment",
      items: [
        {
          label: "Payment term",
          value: formatInvoiceDate(invoice.paymentTerm),
        },
        {
          label: "Payment date",
          value: formatInvoiceDate(invoice.paymentDate),
        },
        {
          label: "Payment time",
          value: formatInvoiceDateTime(invoice.paymentTime),
        },
        { label: "Payment method", value: formatInvoiceText(invoice.paymentMethod) },
      ],
    },
  ];

  return (
    <Modal
      allowSwipeDismissal
      animationType="slide"
      onRequestClose={onClose}
      presentationStyle="pageSheet"
      visible
    >
      <SafeAreaView
        edges={["top", "bottom"]}
        style={[
          invoiceDetailsModalStyles.container,
          { backgroundColor: colorPalette.background },
        ]}
      >
        <View
          style={[
            invoiceDetailsModalStyles.header,
            { borderBottomColor: `${colorPalette.secondary}55` },
          ]}
        >
          <View style={invoiceDetailsModalStyles.heading}>
            <Text
              style={[
                invoiceDetailsModalStyles.eyebrow,
                { color: colorPalette.primary },
              ]}
            >
              Invoice details
            </Text>
            <Text
              style={[
                invoiceDetailsModalStyles.title,
                { color: colorPalette.text },
              ]}
            >
              {invoice.invoiceNumber ?? `Invoice #${invoice.numberId}`}
            </Text>
          </View>
          <Pressable
            accessibilityLabel="Close invoice details"
            accessibilityRole="button"
            onPress={onClose}
            style={({ pressed }) => [
              invoiceDetailsModalStyles.closeButton,
              { backgroundColor: `${colorPalette.primary}18` },
              pressed ? invoiceDetailsModalStyles.pressed : null,
            ]}
          >
            <Ionicons color={colorPalette.primary} name="close" size={25} />
          </Pressable>
        </View>

        <ScrollView contentContainerStyle={invoiceDetailsModalStyles.content}>
          {sections.map((section) => (
            <View key={section.title} style={invoiceDetailsModalStyles.section}>
              <Text
                style={[
                  invoiceDetailsModalStyles.sectionTitle,
                  { color: colorPalette.text },
                ]}
              >
                {section.title}
              </Text>
              <View style={invoiceDetailsModalStyles.detailGrid}>
                {section.items.map((item) => (
                  <View
                    key={item.label}
                    style={[
                      invoiceDetailsModalStyles.detailRow,
                      { borderBottomColor: `${colorPalette.secondary}44` },
                    ]}
                  >
                    <Text
                      style={[
                        invoiceDetailsModalStyles.label,
                        { color: colorPalette.secondary },
                      ]}
                    >
                      {item.label}
                    </Text>
                    <Text
                      selectable
                      style={[
                        invoiceDetailsModalStyles.value,
                        { color: colorPalette.text },
                      ]}
                    >
                      {item.value}
                    </Text>
                  </View>
                ))}
              </View>
            </View>
          ))}
        </ScrollView>
      </SafeAreaView>
    </Modal>
  );
};

export default InvoiceDetailsModal;
