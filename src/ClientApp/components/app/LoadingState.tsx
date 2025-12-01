import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { ActivityIndicator, Text } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";

type LoadingStateProps = {
  label?: string;
};

const LoadingState = ({ label = "Loading..." }: LoadingStateProps) => {
  const colorPalette = useColorPalette();

  return (
    <SafeAreaView
      style={{
        flex: 1,
        justifyContent: "center",
        alignItems: "center",
        gap: 12,
      }}
    >
      <ActivityIndicator size="large" color={colorPalette.text} />
      <Text style={{ color: colorPalette.text }}>{label}</Text>
    </SafeAreaView>
  );
};

export default LoadingState;
