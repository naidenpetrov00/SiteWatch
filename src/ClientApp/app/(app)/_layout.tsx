import {
  Badge,
  Icon,
  Label,
  NativeTabs,
} from "expo-router/unstable-native-tabs";
import { DynamicColorIOS, Text } from "react-native";

import React from "react";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function AppLayout() {
  const colorPalette = useColorPalette();
  return (
    <NativeTabs
      labelStyle={{ color: colorPalette.text }}
      tintColor={colorPalette.tabIconSelected}
    >
      <NativeTabs.Trigger name="Sites">
        <Icon sf="gear" drawable="custom_settings_drawable" />
        <Label>Sites</Label>
        <Badge>2 Cams</Badge>
      </NativeTabs.Trigger>
      <NativeTabs.Trigger name="index">
        <Label>Home</Label>
        <Icon sf="house.fill" drawable="custom_android_drawable" />
      </NativeTabs.Trigger>
    </NativeTabs>
  );
}
