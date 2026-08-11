import { useCallback, useMemo, useState } from "react";
import {
  FlatList,
  Linking,
  Text,
  View,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { env } from "@/config/env";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useColorPalette } from "@/hooks/useColorPalette";
import { UPLOAD_ACTION_BOTTOM_CLEARANCE } from "@/features/sites/info/uploads/constants";
import SiteUploadAction, { type UploadSourceOption } from "@/features/sites/info/uploads/SiteUploadAction";
import type { UploadAsset } from "@/features/sites/info/uploads/types";
import type { SiteInvoice } from "../../types";
import { useGetSiteInvoices } from "../../hooks/useGetSiteInvoices";
import {
  useCreateInvoiceFromFile,
  usePendingInvoiceUploads,
} from "../../hooks/useCreateInvoiceFromFile";
import { useInvoiceFileAccess } from "../../hooks/useInvoiceFileAccess";
import InvoiceCard from "../InvoiceCard/InvoiceCard";
import InvoiceDetailsModal from "../InvoiceDetailsModal/InvoiceDetailsModal";
import PendingInvoiceUploadCard from "../InvoiceCard/PendingInvoiceUploadCard";
import invoicesStyles from "./Invoices.styles";

const MAX_INVOICE_FILE_SIZE = 20 * 1024 * 1024;
const ALLOWED_CONTENT_TYPES = new Set([
  "application/pdf",
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/gif",
  "image/heic",
  "image/heif",
]);
const INVOICE_UPLOAD_SOURCES: readonly UploadSourceOption[] = [
  { source: "file", label: "Browse files" },
];

type InvoiceListItem =
  | { kind: "pending"; mutationId: number; fileName: string }
  | { kind: "invoice"; invoice: SiteInvoice };

const contentTypeFromFileName = (fileName: string): string | null => {
  const extension = fileName.split(".").pop()?.toLowerCase();
  const types: Record<string, string> = {
    pdf: "application/pdf",
    jpg: "image/jpeg",
    jpeg: "image/jpeg",
    png: "image/png",
    webp: "image/webp",
    gif: "image/gif",
    heic: "image/heic",
    heif: "image/heif",
  };
  return extension ? types[extension] ?? null : null;
};

const resolveInvoiceContentType = (
  contentType: string | null | undefined,
  fileName: string,
) => {
  const normalised = contentType === "image/jpg" ? "image/jpeg" : contentType?.toLowerCase();
  return normalised ?? contentTypeFromFileName(fileName);
};

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
  const createFromFile = useCreateInvoiceFromFile();
  const pendingUploads = usePendingInvoiceUploads(siteId);

  const selectedInvoice = useMemo(
    () => invoices.find((invoice) => invoice.id === selectedInvoiceId) ?? null,
    [invoices, selectedInvoiceId],
  );
  const openingInvoiceId = isOpeningFile
    ? fileAccessVariables?.invoiceId ?? null
    : null;
  const displayInvoices = useMemo<InvoiceListItem[]>(
    () => [
      ...pendingUploads.map(({ mutationId, request }) => ({
        kind: "pending" as const,
        mutationId,
        fileName: request.file.fileName,
      })),
      ...invoices.map((invoice) => ({ kind: "invoice" as const, invoice })),
    ],
    [invoices, pendingUploads],
  );

  const handleSelectInvoice = useCallback((invoiceId: string) => {
    setSelectedInvoiceId(invoiceId);
  }, []);

  const handleOpenFile = useCallback(
    async (invoiceId: string) => {
      if (!siteId) return;

      setOpenError(null);
      try {
        const access = await requestFileAccess({ siteId, invoiceId });
        await Linking.openURL(new URL(access.url, env.API_URL).toString());
      } catch {
        const invoice = invoices.find((item) => item.id === invoiceId);
        setOpenError(`Unable to open invoice ${invoice?.invoiceNumber ?? "file"}.`);
      }
    },
    [invoices, requestFileAccess, siteId],
  );

  const validateInvoiceAsset = useCallback((asset: UploadAsset): string | null => {
    if (asset.fileSize === 0) return "Choose a non-empty invoice file.";
    if (!ALLOWED_CONTENT_TYPES.has(asset.contentType)) {
      return "Choose a PDF or supported image file.";
    }
    if (asset.fileSize !== undefined && asset.fileSize > MAX_INVOICE_FILE_SIZE) {
      return "The invoice file cannot exceed 20 MB.";
    }
    return null;
  }, []);

  const uploadInvoice = useCallback(
    (asset: UploadAsset) => createFromFile.mutateAsync({ siteId: siteId!, file: asset }),
    [createFromFile, siteId],
  );

  const renderInvoice = useCallback(
    ({ item }: { item: InvoiceListItem }) => {
      if (item.kind === "pending") {
        return <PendingInvoiceUploadCard fileName={item.fileName} />;
      }

      const invoice = item.invoice;
      return (
        <InvoiceCard
          allocatedAmount={invoice.siteAllocation?.amount ?? null}
          date={invoice.date}
          id={invoice.id}
          isComplete={invoice.isComplete}
          numberId={invoice.numberId}
          invoiceNumber={invoice.invoiceNumber}
          isFileActionDisabled={openingInvoiceId !== null}
          isOpeningFile={openingInvoiceId === invoice.id}
          onOpenFile={handleOpenFile}
          onSelect={handleSelectInvoice}
          supplierDisplayLabel={invoice.supplierDisplayLabel}
          submittedFromSiteName={invoice.submittedFromSiteName}
          created={invoice.created}
          totalValueIncludingVat={invoice.totalValueIncludingVat}
        />
      );
    },
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
        <Text style={[invoicesStyles.title, { color: colorPalette.text }]}>
          Site Invoices
        </Text>
        <Text style={[invoicesStyles.subtitle, { color: colorPalette.secondary }]}>
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

      <FlatList<InvoiceListItem>
        contentContainerStyle={[
          invoicesStyles.listContent,
          { paddingBottom: insets.bottom + UPLOAD_ACTION_BOTTOM_CLEARANCE },
        ]}
        data={displayInvoices}
        keyExtractor={(item) =>
          item.kind === "pending"
            ? `pending-invoice-${item.mutationId}`
            : item.invoice.id
        }
        ListEmptyComponent={
          <View
            style={[
              invoicesStyles.emptyState,
              { borderColor: `${colorPalette.secondary}55` },
            ]}
          >
            <Text style={[invoicesStyles.emptyTitle, { color: colorPalette.text }]}>
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

      <SiteUploadAction
        documentPickerTypes={[...ALLOWED_CONTENT_TYPES]}
        fallbackFileName={() => `invoice-${Date.now()}.jpg`}
        isUploading={createFromFile.isPending}
        label="invoice"
        onUpload={uploadInvoice}
        resolveContentType={resolveInvoiceContentType}
        siteId={siteId}
        sourceOptions={INVOICE_UPLOAD_SOURCES}
        validateAsset={validateInvoiceAsset}
      />

      <InvoiceDetailsModal
        invoice={selectedInvoice}
        onClose={() => setSelectedInvoiceId(null)}
      />
    </View>
  );
};

export default Invoices;
