import { FlatList, RefreshControl } from "react-native";

import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import SiteCard from "@/features/sites/component/SiteCard/SiteCard";
import { Site } from "@/features/sites/api/types";
import signUpStyles from "@/features/auth/components/SignUp.styles"; // reuse layout
import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";
import { useRouter } from "expo-router";
import LoadingState from "@/components/app/LoadingState";

const Sites = () => {
  const colorPalette = useColorPalette();
  const router = useRouter();
  const { data: sites, isLoading, isRefetching, refetch } = useGetSitesByUserId();
  const handleSitePress = (site: Site) => {
    router.push({
      pathname: "/Site/[siteId]/Cameras",
      params: {
        siteId: String(site.id),
        siteName: String(site.name ?? ""),
      },
    });
  };

  if (isLoading) {
    return <LoadingState label="Loading sites..." />;
  }

  return (
    <SafeAreaView
      style={{
        flex: 1,
        backgroundColor: colorPalette.background,
        paddingHorizontal: 24,
      }}
    >
      <FlatList
        data={sites}
        keyExtractor={(item) => item.id}
        refreshControl={
          <RefreshControl
            refreshing={isRefetching}
            onRefresh={refetch}
            tintColor={colorPalette.primary}
            colors={[colorPalette.primary]}
          />
        }
        contentContainerStyle={{ paddingVertical: 16 }}
        renderItem={({ item }) => (
          <SiteCard
            site={item}
            onPress={handleSitePress}
          />
        )}
      />
    </SafeAreaView>
  );
};

export default Sites;
