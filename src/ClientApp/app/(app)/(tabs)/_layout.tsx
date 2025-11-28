import { StyleSheet, useColorScheme } from "react-native";

import { BlurView } from "expo-blur";
import Ionicons from "@expo/vector-icons/Ionicons";
import React from "react";
import { Tabs } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function SitesLayout() {
  const colorPalette = useColorPalette();
  const colorScheme = useColorScheme();

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
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
