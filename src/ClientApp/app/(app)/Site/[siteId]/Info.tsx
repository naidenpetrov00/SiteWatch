import { FlatList, Text, View } from "react-native";
import { useGlobalSearchParams, useLocalSearchParams } from "expo-router";

import CameraCard from "@/features/cameras/components/CameraCard/CameraCard";
import ComingSoon from "@/components/ui/ComingSoon";
// app/sites/[siteId].tsx
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { useColorPalette } from "@/hooks/useColorPalette";

const Info = () => {
  const colorPalette = useColorPalette();

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colorPalette.background }}>
      <ComingSoon />
    </SafeAreaView>
  );
};

export default Info;
