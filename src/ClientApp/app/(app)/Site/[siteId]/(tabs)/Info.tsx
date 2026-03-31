import Details from "@/features/sites/info/component/Details/Details/Details";
import React from "react";
import { ScrollView } from "react-native";
import Summary from "@/features/sites/info/component/Summary/Summary";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useSafeAreaInsets } from "react-native-safe-area-context";

const Info = () => {
  const colorPalette = useColorPalette();
  const insets = useSafeAreaInsets();

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
    >
      <Summary />
      <Details />
    </ScrollView>
  );
};

export default Info;
