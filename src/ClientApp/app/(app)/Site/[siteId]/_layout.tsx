import {
  Label,
  NativeTabs,
} from "expo-router/unstable-native-tabs";

import React from "react";
import { useLocalSearchParams } from "expo-router";

export default function SiteTabsLayout() {
  const { siteId } = useLocalSearchParams<{ siteId: string }>();

  return (
    <NativeTabs>
      <NativeTabs.Trigger name="Cameras">
        <Label>Cameras</Label>
      </NativeTabs.Trigger>
      <NativeTabs.Trigger name="Info">
        <Label>Info</Label>
      </NativeTabs.Trigger>
    </NativeTabs>
  );
}
