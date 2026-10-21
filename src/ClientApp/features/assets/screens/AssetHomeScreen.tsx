import Ionicons from "@expo/vector-icons/Ionicons";
import { useRouter } from "expo-router";
import { memo, useCallback, useState } from "react";
import {
  ActivityIndicator,
  FlatList,
  Pressable,
  RefreshControl,
  Text,
  TextInput,
  View,
} from "react-native";

import { getAssetApiError, useSearchMobileAssets } from "../api";
import assetStyles from "../assets.styles";
import type { MobileAssetListItem } from "../types";
import { useColorPalette } from "@/hooks/useColorPalette";

type AssetRowProps = MobileAssetListItem & { onPress: (id: string) => void };

const AssetRow = memo(function AssetRow({
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
}: AssetRowProps) {
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

export default function AssetHomeScreen() {
  const colors = useColorPalette();
  const router = useRouter();
  const [input, setInput] = useState("");
  const [search, setSearch] = useState("");
  const [assignedToCurrentUser, setAssignedToCurrentUser] = useState(false);
  const [pageIndex, setPageIndex] = useState(0);
  const query = useSearchMobileAssets(search, assignedToCurrentUser, pageIndex);

  const applySearch = useCallback(() => {
    setPageIndex(0);
    setSearch(input.trim());
  }, [input]);
  const clearSearch = useCallback(() => {
    setInput("");
    setSearch("");
    setPageIndex(0);
  }, []);
  const selectFilter = useCallback((assigned: boolean) => {
    setAssignedToCurrentUser(assigned);
    setPageIndex(0);
  }, []);
  const openAsset = useCallback(
    (id: string) => router.push({ pathname: "/Assets/[id]", params: { id } }),
    [router],
  );
  const renderItem = useCallback(
    ({ item }: { item: MobileAssetListItem }) => <AssetRow {...item} onPress={openAsset} />,
    [openAsset],
  );

  const items = query.data?.items ?? [];
  const hasNext = (pageIndex + 1) * 20 < (query.data?.filteredCount ?? 0);
  const emptyMessage = search
    ? "No accessible active assets match this search."
    : assignedToCurrentUser
      ? "No accessible active assets are assigned to you."
      : "No accessible active assets are available.";

  return (
    <FlatList
      contentContainerStyle={assetStyles.listContent}
      contentInsetAdjustmentBehavior="automatic"
      data={items}
      keyboardShouldPersistTaps="handled"
      keyExtractor={(item) => item.id}
      refreshControl={
        <RefreshControl
          colors={[colors.primary]}
          onRefresh={() => void query.refetch()}
          refreshing={query.isRefetching}
          tintColor={colors.primary}
        />
      }
      renderItem={renderItem}
      style={[assetStyles.screen, { backgroundColor: colors.background }]}
      ListHeaderComponent={
        <View style={assetStyles.listHeader}>
          <View style={{ gap: 6 }}>
            <Text style={[assetStyles.title, { color: colors.text }]}>Asset tracking</Text>
            <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
              Browse approved assets you can access, scan a QR code, or register a new asset.
            </Text>
          </View>
          <View style={assetStyles.buttonRow}>
            <Pressable
              accessibilityRole="button"
              onPress={() => router.push("/Assets/scan")}
              style={({ pressed }) => [
                assetStyles.button,
                { backgroundColor: colors.primary, opacity: pressed ? 0.78 : 1 },
              ]}
            >
              <Ionicons color={colors.background} name="qr-code-outline" size={18} />
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Scan QR code</Text>
            </Pressable>
            <Pressable
              accessibilityRole="button"
              onPress={() => router.push("/Assets/register")}
              style={({ pressed }) => [
                assetStyles.button,
                { borderColor: colors.primary, borderWidth: 1, opacity: pressed ? 0.78 : 1 },
              ]}
            >
              <Ionicons color={colors.primary} name="add-circle-outline" size={18} />
              <Text style={[assetStyles.buttonText, { color: colors.primary }]}>Register asset</Text>
            </Pressable>
          </View>
          <TextInput
            accessibilityLabel="Asset search"
            autoCapitalize="none"
            maxLength={200}
            onChangeText={setInput}
            onSubmitEditing={applySearch}
            placeholder="Search code, name, make, model, or serial"
            placeholderTextColor={colors.placeholderText}
            returnKeyType="search"
            style={[assetStyles.input, { borderColor: colors.secondary, color: colors.text }]}
            value={input}
          />
          <View style={assetStyles.buttonRow}>
            <Pressable
              accessibilityRole="button"
              disabled={query.isFetching}
              onPress={applySearch}
              style={({ pressed }) => [
                assetStyles.button,
                {
                  backgroundColor: colors.primary,
                  opacity: query.isFetching ? 0.5 : pressed ? 0.78 : 1,
                },
              ]}
            >
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Search</Text>
            </Pressable>
            {search ? (
              <Pressable
                accessibilityRole="button"
                disabled={query.isFetching}
                onPress={clearSearch}
                style={({ pressed }) => [
                  assetStyles.button,
                  {
                    borderColor: colors.secondary,
                    borderWidth: 1,
                    opacity: query.isFetching ? 0.5 : pressed ? 0.78 : 1,
                  },
                ]}
              >
                <Text style={[assetStyles.buttonText, { color: colors.text }]}>Clear</Text>
              </Pressable>
            ) : null}
          </View>
          <View style={assetStyles.choiceRow}>
            <Pressable
              accessibilityRole="button"
              accessibilityState={{ selected: !assignedToCurrentUser }}
              onPress={() => selectFilter(false)}
              style={({ pressed }) => [
                assetStyles.choice,
                {
                  backgroundColor: !assignedToCurrentUser ? colors.primary : colors.background,
                  borderColor: colors.primary,
                  opacity: pressed ? 0.78 : 1,
                },
              ]}
            >
              <Text
                style={[
                  assetStyles.choiceText,
                  { color: !assignedToCurrentUser ? colors.background : colors.primary },
                ]}
              >
                All accessible
              </Text>
            </Pressable>
            <Pressable
              accessibilityRole="button"
              accessibilityState={{ selected: assignedToCurrentUser }}
              onPress={() => selectFilter(true)}
              style={({ pressed }) => [
                assetStyles.choice,
                {
                  backgroundColor: assignedToCurrentUser ? colors.primary : colors.background,
                  borderColor: colors.primary,
                  opacity: pressed ? 0.78 : 1,
                },
              ]}
            >
              <Text
                style={[
                  assetStyles.choiceText,
                  { color: assignedToCurrentUser ? colors.background : colors.primary },
                ]}
              >
                Assigned to me
              </Text>
            </Pressable>
          </View>
          {query.isFetching && !query.isLoading ? <ActivityIndicator color={colors.primary} /> : null}
          {query.isError ? (
            <View style={assetStyles.errorBox}>
              <Text accessibilityRole="alert" style={assetStyles.errorText}>
                {getAssetApiError(query.error, "Assets could not be loaded.")}
              </Text>
              <Pressable accessibilityRole="button" onPress={() => void query.refetch()}>
                <Text style={{ color: colors.primary, fontWeight: "700" }}>Retry</Text>
              </Pressable>
            </View>
          ) : null}
        </View>
      }
      ListEmptyComponent={
        query.isLoading ? (
          <View style={assetStyles.emptyListState}>
            <ActivityIndicator color={colors.primary} />
            <Text style={[assetStyles.centerText, { color: colors.secondary }]}>Loading assets…</Text>
          </View>
        ) : !query.isError ? (
          <Text style={[assetStyles.centerText, { color: colors.secondary }]}>{emptyMessage}</Text>
        ) : null
      }
      ListFooterComponent={
        query.data ? (
          <View style={assetStyles.pagination}>
            <Pressable
              accessibilityRole="button"
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
              accessibilityRole="button"
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
