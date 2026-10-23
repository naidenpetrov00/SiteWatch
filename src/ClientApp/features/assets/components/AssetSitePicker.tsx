import { useDeferredValue, useEffect, useMemo, useState } from "react";
import {
  ActivityIndicator,
  Modal,
  Pressable,
  SectionList,
  Text,
  TextInput,
  View,
} from "react-native";

import { getAssetApiError, useMobileAssetSites } from "../api";
import assetStyles from "../assets.styles";
import type { MobileAssetSite } from "../types";
import { useColorPalette } from "@/hooks/useColorPalette";

type Props = {
  onClose: () => void;
  onSelect: (site: MobileAssetSite) => void;
  visible: boolean;
};

type SiteSection = {
  title: string;
  data: MobileAssetSite[];
};

export default function AssetSitePicker({ onClose, onSelect, visible }: Props) {
  const colors = useColorPalette();
  const [search, setSearch] = useState("");
  const deferredSearch = useDeferredValue(search);
  const query = useMobileAssetSites(deferredSearch, visible);
  const siteSections = useMemo<SiteSection[]>(() => {
    const sites = query.data ?? [];
    const companyStockSites = sites.filter((site) => site.isCompanyStock);
    const ordinarySites = sites.filter((site) => !site.isCompanyStock);

    return [
      ...(companyStockSites.length > 0
        ? [{ title: "Company Stock", data: companyStockSites }]
        : []),
      ...(ordinarySites.length > 0
        ? [{ title: "Ordinary Sites", data: ordinarySites }]
        : []),
    ];
  }, [query.data]);

  useEffect(() => {
    if (visible) {
      setSearch("");
    }
  }, [visible]);

  const close = () => {
    setSearch("");
    onClose();
  };

  const selectSite = (site: MobileAssetSite) => {
    setSearch("");
    onSelect(site);
  };

  return (
    <Modal
      animationType="slide"
      onRequestClose={close}
      presentationStyle="pageSheet"
      visible={visible}
    >
      <View style={[assetStyles.modal, { backgroundColor: colors.background }]}>
        <View style={assetStyles.modalHeader}>
          <Text style={[assetStyles.title, { color: colors.text }]}>Choose a Site</Text>
          <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>Company Stock and ordinary Sites available to your account are shown separately.</Text>
          <TextInput
            accessibilityLabel="Search accessible Sites"
            autoCapitalize="words"
            maxLength={100}
            onChangeText={setSearch}
            placeholder="Search by name or address"
            placeholderTextColor={colors.placeholderText}
            returnKeyType="search"
            style={[assetStyles.input, { borderColor: colors.secondary, color: colors.text }]}
            value={search}
          />
        </View>
        {query.isLoading ? (
          <View style={assetStyles.centerState}>
            <ActivityIndicator color={colors.primary} size="large" />
          </View>
        ) : query.isError ? (
          <View style={assetStyles.centerState}>
            <Text accessibilityRole="alert" style={[assetStyles.centerText, { color: colors.text }]}>
              {getAssetApiError(query.error, "Accessible Sites could not be loaded.")}
            </Text>
            <Pressable
              accessibilityRole="button"
              onPress={() => void query.refetch()}
              style={({ pressed }) => [
                assetStyles.button,
                { backgroundColor: colors.primary, opacity: pressed ? 0.78 : 1 },
              ]}
            >
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Try again</Text>
            </Pressable>
          </View>
        ) : (
          <SectionList<MobileAssetSite, SiteSection>
            contentContainerStyle={assetStyles.listContent}
            keyboardShouldPersistTaps="handled"
            keyExtractor={(site) => site.id}
            ListEmptyComponent={
              <Text style={[assetStyles.centerText, { color: colors.secondary }]}>
                {search.trim().length > 0
                  ? "No accessible Sites match this search."
                  : "No accessible Sites are available."}
              </Text>
            }
            renderSectionHeader={({ section }) => (
              <View style={assetStyles.siteSectionHeader}>
                <Text style={[assetStyles.siteSectionHeaderText, { color: colors.text }]}>{section.title}</Text>
              </View>
            )}
            renderItem={({ item }) => (
              <Pressable
                accessibilityRole="button"
                onPress={() => selectSite(item)}
                style={({ pressed }) => [
                  assetStyles.listItem,
                  { borderColor: colors.secondary, opacity: pressed ? 0.78 : 1 },
                ]}
              >
                <Text style={[assetStyles.listItemTitle, { color: colors.text }]}>{item.name}</Text>
                <Text style={[assetStyles.metadata, { color: colors.secondary }]}>{item.address}</Text>
                {item.isCompanyStock ? (
                  <View style={[assetStyles.companyStockBadge, { borderColor: colors.primary }]}>
                    <Text style={[assetStyles.companyStockBadgeText, { color: colors.primary }]}>Company Stock</Text>
                  </View>
                ) : null}
              </Pressable>
            )}
            sections={siteSections}
          />
        )}
        <View style={assetStyles.modalFooter}>
          <Pressable
            accessibilityRole="button"
            onPress={close}
            style={({ pressed }) => [
              assetStyles.button,
              { backgroundColor: colors.secondary, opacity: pressed ? 0.78 : 1 },
            ]}
          >
            <Text style={[assetStyles.buttonText, { color: colors.background }]}>Cancel</Text>
          </Pressable>
        </View>
      </View>
    </Modal>
  );
}
