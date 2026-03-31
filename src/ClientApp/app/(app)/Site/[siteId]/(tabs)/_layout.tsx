import { Icon, Label, NativeTabs } from "expo-router/unstable-native-tabs";
import { Platform, StyleSheet, useColorScheme } from "react-native";

import { BlurView } from "expo-blur";
import Ionicons from "@expo/vector-icons/Ionicons";
import React from "react";
import { Tabs } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function SiteTabsLayout() {
  const colorPalette = useColorPalette();
  const colorScheme = useColorScheme();

  if (Platform.OS === "ios") {
    return (
      <NativeTabs
        blurEffect="systemUltraThinMaterial"
        backgroundColor="transparent"
        shadowColor="transparent"
        disableTransparentOnScrollEdge
        badgeTextColor={colorPalette.primary}
        iconColor={colorPalette.primary}
      >
        <NativeTabs.Trigger name="Cameras">
          <Icon sf="camera" />
          <Label>Cameras</Label>
        </NativeTabs.Trigger>

        <NativeTabs.Trigger name="Info">
          <Icon sf="info.circle" />
          <Label>Info</Label>
        </NativeTabs.Trigger>
      </NativeTabs>
    );
  }

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colorPalette.tabIconSelected,
        tabBarInactiveTintColor: colorPalette.tabIconDefault,
        tabBarStyle: {
          position: "absolute",
          backgroundColor: "transparent",
          borderTopWidth: 0,
          elevation: 0,
        },
        tabBarBackground: () => (
          <BlurView
            tint={colorScheme ?? "light"}
            intensity={50}
            style={StyleSheet.absoluteFill}
          />
        ),
      }}
    >
      <Tabs.Screen
        name="Cameras"
        options={{
          title: "Cameras",
          tabBarIcon: ({ color, focused }) => (
            <Ionicons
              name={focused ? "camera" : "camera-outline"}
              size={20}
              color={color}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="Info"
        options={{
          title: "Info",
          tabBarIcon: ({ color, focused }) => (
            <Ionicons
              name={
                focused ? "information-circle" : "information-circle-outline"
              }
              size={20}
              color={color}
            />
          ),
        }}
      />
    </Tabs>
  );
}
