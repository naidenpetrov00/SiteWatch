import { useRouter } from "expo-router";
import { memo, useCallback, useState } from "react";
import {
  ActivityIndicator,
  FlatList,
  Pressable,
  Text,
  TextInput,
  View,
} from "react-native";

import { getAssetApiError, useSearchMobileAssets } from "../api";
import assetStyles from "../assets.styles";
import type { MobileAssetListItem } from "../types";
import { useColorPalette } from "@/hooks/useColorPalette";

type ResultRowProps = MobileAssetListItem & { onPress: (id: string) => void };

const ResultRow = memo(function ResultRow({
  id,
  assetCode,
  classification,
  name,
  manufacturer,
  model,
  manufacturerSerialNumber,
  siteName,
  condition,
  operationalStatus,
  onPress,
}: ResultRowProps) {
  const colors = useColorPalette();
  const open = useCallback(() => onPress(id), [id, onPress]);
  const makeAndModel = [manufacturer, model].filter(Boolean).join(" · ");

  return (
    <Pressable
      accessibilityRole="button"
      onPress={open}
      style={({ pressed }) => [
        assetStyles.listItem,
        { borderColor: colors.secondary, opacity: pressed ? 0.78 : 1 },
      ]}
    >
      <Text style={[assetStyles.listItemTitle, { color: colors.text }]}>{name}</Text>
      <Text style={[assetStyles.metadata, { color: colors.secondary }]}>
        {assetCode} · {classification} · {siteName}
      </Text>
      {makeAndModel ? (
        <Text style={[assetStyles.metadata, { color: colors.secondary }]}>{makeAndModel}</Text>
      ) : null}
      {manufacturerSerialNumber ? (
        <Text style={[assetStyles.metadata, { color: colors.secondary }]}>
          Serial: {manufacturerSerialNumber}
        </Text>
      ) : null}
      <Text style={[assetStyles.metadata, { color: colors.secondary }]}>
        {condition} · {operationalStatus}
      </Text>
    </Pressable>
  );
});

export default function AssetSearchScreen() {
  const colors = useColorPalette();
  const router = useRouter();
  const [input, setInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageIndex, setPageIndex] = useState(0);
  const query = useSearchMobileAssets(search, false, pageIndex);

  const submit = useCallback(() => {
    const normalized = input.trim();
    if (!normalized) return;
    setPageIndex(0);
    setSearch(normalized);
  }, [input]);
  const openAsset = useCallback(
    (id: string) => router.push({ pathname: "/Assets/[id]", params: { id } }),
    [router],
  );
  const renderItem = useCallback(
    ({ item }: { item: MobileAssetListItem }) => (
      <ResultRow {...item} onPress={openAsset} />
    ),
    [openAsset],
  );
  const items = query.data?.items ?? [];
  const hasNext = (pageIndex + 1) * 20 < (query.data?.filteredCount ?? 0);

  return (
    <FlatList
      contentContainerStyle={assetStyles.listContent}
      contentInsetAdjustmentBehavior="automatic"
      data={items}
      keyboardShouldPersistTaps="handled"
      keyExtractor={(item) => item.id}
      renderItem={renderItem}
      style={[assetStyles.screen, { backgroundColor: colors.background }]}
      ListHeaderComponent={
        <View style={assetStyles.listHeader}>
          <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
            Search by asset code or number, name, manufacturer, model, or serial number.
          </Text>
          <TextInput
            accessibilityLabel="Asset search"
            autoCapitalize="none"
            maxLength={200}
            onChangeText={setInput}
            onSubmitEditing={submit}
            placeholder="AST-000123 or cordless drill"
            placeholderTextColor={colors.placeholderText}
            returnKeyType="search"
            style={[assetStyles.input, { borderColor: colors.secondary, color: colors.text }]}
            value={input}
          />
          <Pressable
            accessibilityRole="button"
            disabled={!input.trim() || query.isFetching}
            onPress={submit}
            style={({ pressed }) => [
              assetStyles.button,
              {
                backgroundColor: colors.primary,
                opacity: !input.trim() || query.isFetching ? 0.5 : pressed ? 0.78 : 1,
              },
            ]}
          >
            <Text style={[assetStyles.buttonText, { color: colors.background }]}>Search</Text>
          </Pressable>
          {query.isFetching ? <ActivityIndicator color={colors.primary} /> : null}
          {query.isError ? (
            <View style={assetStyles.errorBox}>
              <Text accessibilityRole="alert" style={assetStyles.errorText}>
                {getAssetApiError(query.error, "Assets could not be searched.")}
              </Text>
              <Pressable onPress={() => void query.refetch()}>
                <Text style={{ color: colors.primary, fontWeight: "700" }}>Retry</Text>
              </Pressable>
            </View>
          ) : null}
        </View>
      }
      ListEmptyComponent={
        search && !query.isFetching && !query.isError ? (
          <Text style={[assetStyles.centerText, { color: colors.secondary }]}>
            No accessible active assets match this search.
          </Text>
        ) : null
      }
      ListFooterComponent={
        query.data ? (
          <View style={assetStyles.pagination}>
            <Pressable
              disabled={pageIndex === 0 || query.isFetching}
              onPress={() => setPageIndex((current) => Math.max(0, current - 1))}
              style={({ pressed }) => [
                assetStyles.button,
                {
                  backgroundColor: colors.secondary,
                  opacity: pageIndex === 0 || query.isFetching ? 0.45 : pressed ? 0.78 : 1,
                },
              ]}
            >
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Previous</Text>
            </Pressable>
            <Text style={{ color: colors.text }}>Page {pageIndex + 1}</Text>
            <Pressable
              disabled={!hasNext || query.isFetching}
              onPress={() => setPageIndex((current) => current + 1)}
              style={({ pressed }) => [
                assetStyles.button,
                {
                  backgroundColor: colors.primary,
                  opacity: !hasNext || query.isFetching ? 0.45 : pressed ? 0.78 : 1,
                },
              ]}
            >
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Next</Text>
            </Pressable>
          </View>
        ) : null
      }
    />
  );
}
