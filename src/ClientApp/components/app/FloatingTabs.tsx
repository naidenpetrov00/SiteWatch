import Ionicons from "@expo/vector-icons/Ionicons";
import { Icon, Label, NativeTabs } from "expo-router/unstable-native-tabs";
import { Tabs } from "expo-router";
import type { SFSymbol } from "sf-symbols-typescript";
import { Platform, StyleSheet, useColorScheme, View } from "react-native";

import { BlurView } from "expo-blur";
import type { ComponentProps } from "react";
import { useColorPalette } from "@/hooks/useColorPalette";

type FloatingTab = {
  androidIcon: { selected: ComponentProps<typeof Ionicons>["name"]; unselected: ComponentProps<typeof Ionicons>["name"] };
  iosIcon: SFSymbol;
  label: string;
  name: string;
};

type FloatingTabsProps = {
  headerTintColor?: boolean;
  tabs: readonly FloatingTab[];
};

const FloatingTabBackground = () => {
  const colorPalette = useColorPalette();
  const colorScheme = useColorScheme();

  return <BlurView intensity={100} style={StyleSheet.absoluteFill} tint={colorScheme ?? "light"}><View style={[StyleSheet.absoluteFill, { backgroundColor: `${colorPalette.background}33` }]} /></BlurView>;
};

const FloatingTabs = ({ headerTintColor = false, tabs }: FloatingTabsProps) => {
  const colorPalette = useColorPalette();

  if (Platform.OS === "ios") {
    return <NativeTabs backgroundColor="transparent" badgeTextColor={colorPalette.primary} blurEffect="systemThickMaterial" disableTransparentOnScrollEdge iconColor={colorPalette.primary} shadowColor="transparent">
      {tabs.map((tab) => <NativeTabs.Trigger key={tab.name} name={tab.name}><Icon sf={tab.iosIcon} /><Label>{tab.label}</Label></NativeTabs.Trigger>)}
    </NativeTabs>;
  }

  return <Tabs screenOptions={{
    headerShown: false,
    headerTintColor: headerTintColor ? colorPalette.primary : undefined,
    tabBarActiveTintColor: colorPalette.tabIconSelected,
    tabBarBackground: FloatingTabBackground,
    tabBarInactiveTintColor: colorPalette.tabIconDefault,
    tabBarStyle: { backgroundColor: "transparent", borderTopWidth: 0, elevation: 0, position: "absolute" },
  }}>
    {tabs.map((tab) => <Tabs.Screen key={tab.name} name={tab.name} options={{ title: tab.label, tabBarIcon: ({ color, focused }) => <Ionicons color={color} name={focused ? tab.androidIcon.selected : tab.androidIcon.unselected} size={20} /> }} />)}
  </Tabs>;
};

export default FloatingTabs;
