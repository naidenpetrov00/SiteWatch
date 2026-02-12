import { Icon, Label, NativeTabs } from "expo-router/unstable-native-tabs";
import { Platform, StyleSheet, useColorScheme } from "react-native";

import { BlurView } from "expo-blur";
import Ionicons from "@expo/vector-icons/Ionicons";
import React from "react";
import { Tabs } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function SitesLayout() {
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
        <NativeTabs.Trigger name="Sites">
          <Icon sf="building.2" />
          <Label>Sites</Label>
        </NativeTabs.Trigger>

        <NativeTabs.Trigger name="index">
          <Icon sf="house" />
          <Label>Home</Label>
        </NativeTabs.Trigger>
      </NativeTabs>
    );
  }

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: {
          position: "absolute",
          backgroundColor: "transparent",
          borderTopWidth: 0,
          elevation: 0,
        },
        tabBarActiveTintColor: colorPalette.tabIconSelected,
        tabBarInactiveTintColor: colorPalette.tabIconDefault,
        tabBarBackground: () => (
          <BlurView
            tint={colorScheme ?? "light"}
            intensity={50}
            style={StyleSheet.absoluteFill}
          />
        ),
        headerTintColor: colorPalette.primary,
      }}
    >
      <Tabs.Screen
        name="Sites"
        options={{
          title: "Sites",
          tabBarIcon: ({ color, focused }) => (
            <Ionicons
              name={focused ? "business" : "business-outline"}
              size={20}
              color={color}
            />
          ),
        }}
      />
      <Tabs.Screen
        name="index"
        options={{
          title: "Home",
          tabBarIcon: ({ color, focused }) => (
            <Ionicons
              name={focused ? "home" : "home-outline"}
              size={20}
              color={color}
            />
          ),
        }}
      />
    </Tabs>
  );
}
