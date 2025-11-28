import { StyleSheet, useColorScheme } from "react-native";

import { BlurView } from "expo-blur";
import Ionicons from "@expo/vector-icons/Ionicons";
import React from "react";
import { Tabs } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useLocalSearchParams } from "expo-router";

export default function SiteTabsLayout() {
  useLocalSearchParams<{ siteId: string }>();
  const colorPalette = useColorPalette();
  const colorScheme = useColorScheme();

  return (
    <Tabs
      screenOptions={{
        headerShown:false,
        tabBarStyle: { position: "absolute" },
        tabBarActiveTintColor: colorPalette.tabIconSelected,
        tabBarInactiveTintColor: colorPalette.tabIconDefault,
        tabBarBackground: () => (
          <BlurView
            tint={colorScheme!}
            intensity={50}
            style={StyleSheet.absoluteFill}
          />
        ),
        headerTintColor: colorPalette.primary,
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
