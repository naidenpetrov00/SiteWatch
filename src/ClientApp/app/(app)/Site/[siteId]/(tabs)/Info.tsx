import Details from "@/features/sites/info/component/Details/Details/Details";
import React from "react";
import { RefreshControl, ScrollView } from "react-native";
import Summary from "@/features/sites/info/component/Summary/Summary";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";

const Info = () => {
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();
  const { isRefetching, refetch } = useGetSitesByUserId();

  return (
    <ScrollView
      style={{
        flex: 1,
        backgroundColor: colorPalette.background,
      }}
      contentContainerStyle={{
        paddingBottom: insets.bottom + 80,
        paddingTop: 16,
      }}
      refreshControl={
        <RefreshControl
          refreshing={isRefetching}
          onRefresh={refetch}
          tintColor={colorPalette.primary}
          colors={[colorPalette.primary]}
        />
      }
    >
      <Summary />
      <Details />
    </ScrollView>
  );
};

export default Info;
