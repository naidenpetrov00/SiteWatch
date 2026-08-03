import { useCallback, useMemo, useState } from "react";
import { FlatList, Linking, Text, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useColorPalette } from "@/hooks/useColorPalette";
import type { SiteInvoice } from "../../types";
import { useGetSiteInvoices } from "../../hooks/useGetSiteInvoices";
import { useInvoiceFileAccess } from "../../hooks/useInvoiceFileAccess";
import InvoiceCard from "../InvoiceCard/InvoiceCard";
import InvoiceDetailsModal from "../InvoiceDetailsModal/InvoiceDetailsModal";
import invoicesStyles from "./Invoices.styles";

const Invoices = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | null>(null);
  const [openError, setOpenError] = useState<string | null>(null);
  const {
    data: invoices = [],
    error,
    isError,
    isLoading,
    isRefetching,
    refetch,
  } = useGetSiteInvoices({ siteId });
  const {
    isPending: isOpeningFile,
    mutateAsync: requestFileAccess,
    variables: fileAccessVariables,
  } = useInvoiceFileAccess();

  const selectedInvoice = useMemo(
    () => invoices.find((invoice) => invoice.id === selectedInvoiceId) ?? null,
    [invoices, selectedInvoiceId],
  );
  const openingInvoiceId = isOpeningFile
    ? fileAccessVariables?.invoiceId ?? null
    : null;

  const handleSelectInvoice = useCallback((invoiceId: string) => {
    setSelectedInvoiceId(invoiceId);
  }, []);

  const handleOpenFile = useCallback(
    async (invoiceId: string) => {
      if (!siteId) return;

      setOpenError(null);
      try {
        const access = await requestFileAccess({
          siteId,
          invoiceId,
        });
        await Linking.openURL(access.url);
      } catch {
        const invoice = invoices.find((item) => item.id === invoiceId);
        setOpenError(
          `Unable to open invoice ${invoice?.invoiceNumber ?? "file"}.`,
        );
      }
    },
    [invoices, requestFileAccess, siteId],
  );

  const renderInvoice = useCallback(
    ({ item }: { item: SiteInvoice }) => (
      <InvoiceCard
        allocatedAmount={item.siteAllocation.amount}
        date={item.date}
        id={item.id}
        invoiceNumber={item.invoiceNumber}
        isFileActionDisabled={openingInvoiceId !== null}
        isOpeningFile={openingInvoiceId === item.id}
        onOpenFile={handleOpenFile}
        onSelect={handleSelectInvoice}
        supplierDisplayLabel={item.supplierDisplayLabel}
        totalValueIncludingVat={item.totalValueIncludingVat}
      />
    ),
    [handleOpenFile, handleSelectInvoice, openingInvoiceId],
  );

  const emptyMessage = isLoading
    ? "Loading invoices..."
    : isError
      ? error instanceof Error
        ? error.message
        : "Invoices could not be retrieved."
      : "No invoices are allocated to this site.";

  return (
    <View
      style={[
        invoicesStyles.container,
        { backgroundColor: colorPalette.background },
      ]}
    >
      <View style={invoicesStyles.header}>
        <Text
          style={[invoicesStyles.title, { color: colorPalette.text }]}
        >
          Site Invoices
        </Text>
        <Text
          style={[invoicesStyles.subtitle, { color: colorPalette.secondary }]}
        >
          Review billing details and open the original invoice file.
        </Text>
        {openError ? (
          <Text
            accessibilityRole="alert"
            style={[
              invoicesStyles.error,
              {
                borderColor: `${colorPalette.primary}66`,
                color: colorPalette.text,
              },
            ]}
          >
            {openError}
          </Text>
        ) : null}
      </View>

      <FlatList<SiteInvoice>
        contentContainerStyle={[
          invoicesStyles.listContent,
          { paddingBottom: insets.bottom + 24 },
        ]}
        data={invoices}
        keyExtractor={(item) => item.id}
        ListEmptyComponent={
          <View
            style={[
              invoicesStyles.emptyState,
              { borderColor: `${colorPalette.secondary}55` },
            ]}
          >
            <Text
              style={[invoicesStyles.emptyTitle, { color: colorPalette.text }]}
            >
              Invoices
            </Text>
            <Text
              style={[
                invoicesStyles.emptyDescription,
                { color: colorPalette.secondary },
              ]}
            >
              {emptyMessage}
            </Text>
          </View>
        }
        onRefresh={() => void refetch()}
        refreshing={isRefetching}
        renderItem={renderInvoice}
        showsVerticalScrollIndicator={false}
      />

      <InvoiceDetailsModal
        invoice={selectedInvoice}
        onClose={() => setSelectedInvoiceId(null)}
      />
    </View>
  );
};

export default Invoices;
