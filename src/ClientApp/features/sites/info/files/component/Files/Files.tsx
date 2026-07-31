import { FlatList, Linking, Pressable, Text, View } from "react-native";
import { useCallback, useEffect, useMemo, useState } from "react";

import { ALL_FILTER, type MediaFilter } from "@/features/sites/info/media-types";
import { env } from "@/config/env";
import type { SiteFileIds } from "../../types";
import { paths } from "@/config/constants/paths";
import { useGetSiteFileIdsBySiteId } from "../../hooks/useGetSiteFileIdsBySiteId";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import filesStyles from "./Files.styles";

const Files = () => {
  const { siteId } = useGetSearchParams<{ siteId?: string }>();
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const { data: sites = [] } = useGetSitesByUserId();
  const [activeFilter, setActiveFilter] = useState<MediaFilter>(ALL_FILTER);
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

  const site = useMemo(
    () => sites.find((siteItem) => siteItem.id === siteId),
    [siteId, sites],
  );
  const filters = useMemo<MediaFilter[]>(
    () => [ALL_FILTER, ...(site?.mediaPolicy.allowedFileCategories ?? [])],
    [site],
  );
  const resolvedActiveFilter = filters.includes(activeFilter)
    ? activeFilter
    : ALL_FILTER;
  const filteredFiles = useMemo(
    () =>
      resolvedActiveFilter === ALL_FILTER
        ? siteFiles
        : siteFiles.filter((file) => file.category === resolvedActiveFilter),
    [resolvedActiveFilter, siteFiles],
  );

  useEffect(() => {
    if (resolvedActiveFilter !== activeFilter) {
      setActiveFilter(ALL_FILTER);
    }
  }, [activeFilter, resolvedActiveFilter]);

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
          {item.category} · {item.contentType}
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
      <View style={filesStyles.filters}>
        {filters.map((filter) => {
          const isActive = filter === resolvedActiveFilter;

          return (
            <Pressable
              key={filter}
              onPress={() => setActiveFilter(filter)}
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
                {filter}
              </Text>
            </Pressable>
          );
        })}
      </View>

      {openError ? (
        <Text style={[filesStyles.description, { color: colorPalette.secondary }]}>
          {openError}
        </Text>
      ) : null}

      <FlatList<SiteFileIds>
        contentContainerStyle={[
          filesStyles.listContent,
          { paddingBottom: insets.bottom + 24 },
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
    </View>
  );
};

export default Files;
