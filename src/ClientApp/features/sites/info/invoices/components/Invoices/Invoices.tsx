import { useCallback, useMemo, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  FlatList,
  Linking,
  Pressable,
  Text,
  View,
} from "react-native";
import * as DocumentPicker from "expo-document-picker";
import * as ImagePicker from "expo-image-picker";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { env } from "@/config/env";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useColorPalette } from "@/hooks/useColorPalette";
import type { InvoiceUploadAsset, SiteInvoice } from "../../types";
import { useGetSiteInvoices } from "../../hooks/useGetSiteInvoices";
import { useInvoiceFileAccess } from "../../hooks/useInvoiceFileAccess";
import { useCreateInvoiceFromFile } from "../../hooks/useCreateInvoiceFromFile";
import InvoiceCard from "../InvoiceCard/InvoiceCard";
import InvoiceDetailsModal from "../InvoiceDetailsModal/InvoiceDetailsModal";
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

const getUploadErrorMessage = (error: unknown, fallback: string): string =>
  error instanceof Error && error.message.trim().length > 0
    ? error.message
    : fallback;

const Invoices = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | null>(null);
  const [openError, setOpenError] = useState<string | null>(null);
  const [uploadActionsVisible, setUploadActionsVisible] = useState(false);
  const [uploadMessage, setUploadMessage] = useState<string | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [activeSource, setActiveSource] = useState<"camera" | "gallery" | "file" | null>(null);
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
        await Linking.openURL(new URL(access.url, env.API_URL).toString());
      } catch {
        const invoice = invoices.find((item) => item.id === invoiceId);
        setOpenError(
          `Unable to open invoice ${invoice?.invoiceNumber ?? "file"}.`,
        );
      }
    },
    [invoices, requestFileAccess, siteId],
  );

  const uploadAsset = useCallback(
    async (asset: InvoiceUploadAsset) => {
      if (!siteId) return;

      if (!ALLOWED_CONTENT_TYPES.has(asset.contentType)) {
        setUploadError("Choose a PDF or supported image file.");
        return;
      }

      if (asset.fileSize !== undefined && asset.fileSize > MAX_INVOICE_FILE_SIZE) {
        setUploadError("The invoice file cannot exceed 20 MB.");
        return;
      }

      setUploadError(null);
      setUploadMessage(null);
      await createFromFile.mutateAsync({ siteId, file: asset });
      setUploadMessage("Invoice uploaded.");
      setUploadActionsVisible(false);
    },
    [createFromFile, siteId],
  );

  const showPermissionDenied = useCallback((source: "Camera" | "Photos") => {
    Alert.alert(`${source} permission required`, `Allow ${source.toLowerCase()} access to add an invoice.`, [
      { text: "Cancel", style: "cancel" },
      { text: "Open Settings", onPress: () => void Linking.openSettings() },
    ]);
  }, []);

  const handleCamera = useCallback(async () => {
    setActiveSource("camera");
    setUploadError(null);
    try {
      const permission = await ImagePicker.requestCameraPermissionsAsync();
      if (!permission.granted) {
        showPermissionDenied("Camera");
        return;
      }

      const result = await ImagePicker.launchCameraAsync({
        mediaTypes: ["images"],
        allowsEditing: false,
        quality: 0.9,
      });
      if (result.canceled) return;

      const asset = result.assets[0];
      const fileName = asset.fileName ?? `invoice-${Date.now()}.jpg`;
      await uploadAsset({
        uri: asset.uri,
        fileName,
        contentType: asset.mimeType ?? contentTypeFromFileName(fileName) ?? "image/jpeg",
        fileSize: asset.fileSize,
      });
    } catch (error) {
      setUploadError(getUploadErrorMessage(error, "Unable to capture or upload the invoice."));
    } finally {
      setActiveSource(null);
    }
  }, [showPermissionDenied, uploadAsset]);

  const handleGallery = useCallback(async () => {
    setActiveSource("gallery");
    setUploadError(null);
    try {
      const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (!permission.granted) {
        showPermissionDenied("Photos");
        return;
      }

      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ["images"],
        allowsEditing: false,
        quality: 1,
      });
      if (result.canceled) return;

      const asset = result.assets[0];
      const fileName = asset.fileName ?? `invoice-${Date.now()}.jpg`;
      await uploadAsset({
        uri: asset.uri,
        fileName,
        contentType: asset.mimeType ?? contentTypeFromFileName(fileName) ?? "image/jpeg",
        fileSize: asset.fileSize,
      });
    } catch (error) {
      setUploadError(getUploadErrorMessage(error, "Unable to select or upload the invoice image."));
    } finally {
      setActiveSource(null);
    }
  }, [showPermissionDenied, uploadAsset]);

  const handleFilePicker = useCallback(async () => {
    setActiveSource("file");
    setUploadError(null);
    try {
      const result = await DocumentPicker.getDocumentAsync({
        type: [...ALLOWED_CONTENT_TYPES],
        multiple: false,
        copyToCacheDirectory: true,
      });
      if (result.canceled) return;

      const asset = result.assets[0];
      const contentType = asset.mimeType ?? contentTypeFromFileName(asset.name);
      if (!contentType) {
        setUploadError("Choose a PDF or supported image file.");
        return;
      }

      await uploadAsset({
        uri: asset.uri,
        fileName: asset.name,
        contentType,
        fileSize: asset.size,
      });
    } catch (error) {
      setUploadError(getUploadErrorMessage(error, "Unable to select or upload the invoice file."));
    } finally {
      setActiveSource(null);
    }
  }, [uploadAsset]);

  const renderInvoice = useCallback(
    ({ item }: { item: SiteInvoice }) => (
      <InvoiceCard
        allocatedAmount={item.siteAllocation?.amount ?? null}
        date={item.date}
        id={item.id}
        isComplete={item.isComplete}
        numberId={item.numberId}
        invoiceNumber={item.invoiceNumber}
        isFileActionDisabled={openingInvoiceId !== null}
        isOpeningFile={openingInvoiceId === item.id}
        onOpenFile={handleOpenFile}
        onSelect={handleSelectInvoice}
        supplierDisplayLabel={item.supplierDisplayLabel}
        submittedFromSiteName={item.submittedFromSiteName}
        created={item.created}
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
        <Pressable
          accessibilityRole="button"
          accessibilityState={{ expanded: uploadActionsVisible }}
          disabled={createFromFile.isPending}
          onPress={() => {
            setUploadActionsVisible((visible) => !visible);
            setUploadError(null);
            setUploadMessage(null);
          }}
          style={({ pressed }) => [
            invoicesStyles.addButton,
            { backgroundColor: colorPalette.primary },
            pressed ? invoicesStyles.pressed : null,
          ]}
        >
          <Text style={[invoicesStyles.addButtonText, { color: colorPalette.contrastText }]}>Add invoice</Text>
        </Pressable>
        {uploadActionsVisible ? (
          <View style={invoicesStyles.sourceActions}>
            {([
              ["camera", "Take a photo", handleCamera],
              ["gallery", "Select from gallery", handleGallery],
              ["file", "Select PDF or image", handleFilePicker],
            ] as const).map(([source, label, action]) => (
              <Pressable
                key={source}
                accessibilityRole="button"
                disabled={createFromFile.isPending || activeSource !== null}
                onPress={() => void action()}
                style={({ pressed }) => [
                  invoicesStyles.sourceButton,
                  { borderColor: `${colorPalette.secondary}88` },
                  pressed ? invoicesStyles.pressed : null,
                ]}
              >
                {activeSource === source ? <ActivityIndicator color={colorPalette.primary} /> : null}
                <Text style={[invoicesStyles.sourceButtonText, { color: colorPalette.text }]}>{label}</Text>
              </Pressable>
            ))}
          </View>
        ) : null}
        {uploadMessage ? (
          <Text accessibilityRole="alert" style={[invoicesStyles.success, { color: colorPalette.text }]}>
            {uploadMessage}
          </Text>
        ) : null}
        {uploadError ? (
          <Text accessibilityRole="alert" style={[invoicesStyles.error, { borderColor: "#b91c1c", color: "#b91c1c" }]}>
            {uploadError}
          </Text>
        ) : null}
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
