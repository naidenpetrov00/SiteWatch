import { FlatList, Linking, Pressable, Text, View } from "react-native";
import { useCallback, useEffect, useMemo, useState } from "react";

import { ALL_FILTER } from "@/features/sites/info/media-types";
import { env } from "@/config/env";
import {
  FILE_DOCUMENT_TYPE_LABELS,
  FILE_DOCUMENT_TYPES,
  type FileDocumentTypeFilter,
  type SiteFileIds,
} from "../../types";
import { paths } from "@/config/constants/paths";
import { useGetSiteFileIdsBySiteId } from "../../hooks/useGetSiteFileIdsBySiteId";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import filesStyles from "./Files.styles";
import SiteMediaUploadAction from "@/features/sites/info/uploads/SiteMediaUploadAction";
import { UPLOAD_ACTION_BOTTOM_CLEARANCE } from "@/features/sites/info/uploads/constants";

const DOCUMENT_TYPE_FILTERS: readonly FileDocumentTypeFilter[] = [
  ALL_FILTER,
  ...FILE_DOCUMENT_TYPES,
];

const Files = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const [activeDocumentTypeFilter, setActiveDocumentTypeFilter] =
    useState<FileDocumentTypeFilter>(ALL_FILTER);
  const [openingFileId, setOpeningFileId] = useState<string | null>(null);
  const [openError, setOpenError] = useState<string | null>(null);
  const {
    data: siteFiles = [],
    error,
    isError,
    isLoading,
    isRefetching,
    refetch,
  } = useGetSiteFileIdsBySiteId({ siteId });

  const filteredFiles = useMemo(
    () =>
      siteFiles.filter(
        (file) =>
          activeDocumentTypeFilter === ALL_FILTER ||
          file.documentType === activeDocumentTypeFilter,
      ),
    [activeDocumentTypeFilter, siteFiles],
  );

  useEffect(() => {
    setOpenError(null);
    setOpeningFileId(null);
  }, [siteId]);

  const handleFilePress = useCallback(async (file: SiteFileIds) => {
    setOpeningFileId(file.fileId);
    setOpenError(null);

    try {
      await Linking.openURL(
        new URL(paths.files.getById(file.fileId), env.API_URL).toString(),
      );
    } catch {
      setOpenError(`Unable to open ${file.fileName}.`);
    } finally {
      setOpeningFileId(null);
    }
  }, []);

  const renderFile = useCallback(
    ({ item }: { item: SiteFileIds }) => (
      <Pressable
        disabled={openingFileId === item.fileId}
        onPress={() => void handleFilePress(item)}
        style={[
          filesStyles.fileRow,
          {
            backgroundColor: `${colorPalette.primary}11`,
            borderColor: colorPalette.secondary + "55",
            opacity: openingFileId === item.fileId ? 0.6 : 1,
          },
        ]}
      >
        <Text style={[filesStyles.fileName, { color: colorPalette.text }]}>
          {item.fileName}
        </Text>
        <Text style={[filesStyles.metadata, { color: colorPalette.secondary }]}>
          Document type: {FILE_DOCUMENT_TYPE_LABELS[item.documentType]}
        </Text>
        <Text style={[filesStyles.metadata, { color: colorPalette.secondary }]}>
          Content type: {item.contentType}
        </Text>
      </Pressable>
    ),
    [colorPalette, handleFilePress, openingFileId],
  );

  const emptyMessage = isLoading
    ? "Loading files..."
    : isError
      ? error instanceof Error
        ? error.message
        : "Files could not be retrieved."
      : "No files in this filter yet.";

  return (
    <View style={filesStyles.container}>
      <Text style={[filesStyles.title, { color: colorPalette.text }]}>
        Site Files
      </Text>
      <Text style={[filesStyles.subtitle, { color: colorPalette.secondary }]}>
        Site ID: {siteId ?? "Unknown"}
      </Text>
      <View style={filesStyles.filterGroup}>
        <Text style={[filesStyles.filterLabel, { color: colorPalette.text }]}>
          Document type
        </Text>
        <View style={filesStyles.filters}>
          {DOCUMENT_TYPE_FILTERS.map((filter) => {
            const isActive = filter === activeDocumentTypeFilter;

            return (
              <Pressable
                key={filter}
                onPress={() => setActiveDocumentTypeFilter(filter)}
                style={[
                  filesStyles.filterChip,
                  {
                    backgroundColor: isActive
                      ? colorPalette.primary
                      : colorPalette.background,
                    borderColor: isActive
                      ? colorPalette.primary
                      : colorPalette.secondary,
                  },
                ]}
              >
                <Text
                  style={[
                    filesStyles.filterText,
                    {
                      color: isActive
                        ? colorPalette.contrastText
                        : colorPalette.text,
                    },
                  ]}
                >
                  {filter === ALL_FILTER
                    ? filter
                    : FILE_DOCUMENT_TYPE_LABELS[filter]}
                </Text>
              </Pressable>
            );
          })}
        </View>
      </View>

      {openError ? (
        <Text style={[filesStyles.description, { color: colorPalette.secondary }]}>
          {openError}
        </Text>
      ) : null}

      <FlatList<SiteFileIds>
        contentContainerStyle={[
          filesStyles.listContent,
          { paddingBottom: insets.bottom + UPLOAD_ACTION_BOTTOM_CLEARANCE },
        ]}
        data={filteredFiles}
        keyExtractor={(item) => item.fileId}
        ListEmptyComponent={
          <View
            style={[
              filesStyles.emptyState,
              { borderColor: colorPalette.secondary + "55" },
            ]}
          >
            <Text style={[filesStyles.description, { color: colorPalette.secondary }]}>
              {emptyMessage}
            </Text>
          </View>
        }
        onRefresh={() => void refetch()}
        refreshing={isRefetching}
        renderItem={renderFile}
        showsVerticalScrollIndicator={false}
      />
      <SiteMediaUploadAction kind="file" siteId={siteId} />
    </View>
  );
};

export default Files;
