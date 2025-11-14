import { FlatList, View } from "react-native";

import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import SiteCard from "@/features/sites/component/SiteCard";
import signUpStyles from "@/features/auth/components/SignUp.styles"; // reuse layout
import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";

const Sites = () => {
  const colorPalette = useColorPalette();

  const { data: sites } = useGetSitesByUserId();

  return (
    <SafeAreaView
      style={[
        signUpStyles.safe,
        { backgroundColor: colorPalette.background, flex: 1 },
      ]}
    >
      <View
        style={[
          signUpStyles.container,
          { backgroundColor: colorPalette.background },
        ]}
      >
        <FlatList
          data={sites}
          keyExtractor={(item) => item.id}
          contentContainerStyle={{ paddingVertical: 16 }}
          renderItem={({ item }) => (
            <SiteCard
              site={item}
              onPress={(site) => {
                console.log("Pressed site:", site.id);
              }}
            />
          )}
        />
      </View>
    </SafeAreaView>
  );
};

export default Sites;
