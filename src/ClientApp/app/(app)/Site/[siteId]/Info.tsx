import Details from "@/features/sites/info/component/Details/Details/Details";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import Summary from "@/features/sites/info/component/Summary/Summary";
import { useColorPalette } from "@/hooks/useColorPalette";

const Info = () => {
  const colorPalette = useColorPalette();

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colorPalette.background }}>
      <Summary />
      <Details />
    </SafeAreaView>
  );
};

export default Info;
