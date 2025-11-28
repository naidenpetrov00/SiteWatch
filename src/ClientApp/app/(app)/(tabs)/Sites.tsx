import { FlatList, View } from "react-native";

import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import SiteCard from "@/features/sites/component/SiteCard/SiteCard";
import signUpStyles from "@/features/auth/components/SignUp.styles"; // reuse layout
import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";
import { useRouter } from "expo-router";

const Sites = () => {
  const colorPalette = useColorPalette();
  const router = useRouter();
  const { data: sites } = useGetSitesByUserId();

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
        contentContainerStyle={{ paddingVertical: 16 }}
        renderItem={({ item }) => (
          <SiteCard
            site={item}
            onPress={(site) => {
              router.push(`/Site/${site.id}`);
            }}
          />
        )}
      />
    </SafeAreaView>
  );
};

export default Sites;
