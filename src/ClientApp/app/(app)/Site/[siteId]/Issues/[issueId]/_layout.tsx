import Ionicons from "@expo/vector-icons/Ionicons";
import { Icon, Label, NativeTabs } from "expo-router/unstable-native-tabs";
import { Platform, StyleSheet, useColorScheme } from "react-native";

import { BlurView } from "expo-blur";
import { Tabs } from "expo-router";
import { useColorPalette } from "@/hooks/useColorPalette";

const IssueTabsLayout = () => {
  const colorPalette = useColorPalette();
  const colorScheme = useColorScheme();

  if (Platform.OS === "ios") {
    return (
      <NativeTabs
        backgroundColor="transparent"
        blurEffect="systemUltraThinMaterial"
        disableTransparentOnScrollEdge
        iconColor={colorPalette.primary}
        shadowColor="transparent"
      >
        <NativeTabs.Trigger name="index">
          <Icon sf="info.circle" />
          <Label>Details</Label>
        </NativeTabs.Trigger>
        <NativeTabs.Trigger name="attachments">
          <Icon sf="paperclip" />
          <Label>Attachments</Label>
        </NativeTabs.Trigger>
      </NativeTabs>
    );
  }

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colorPalette.tabIconSelected,
        tabBarBackground: () => (
          <BlurView
            intensity={50}
            style={StyleSheet.absoluteFill}
            tint={colorScheme ?? "light"}
          />
        ),
        tabBarInactiveTintColor: colorPalette.tabIconDefault,
        tabBarStyle: {
          backgroundColor: "transparent",
          borderTopWidth: 0,
          elevation: 0,
          position: "absolute",
        },
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: "Details",
          tabBarIcon: ({ color, focused }) => (
            <Ionicons color={color} name={focused ? "information-circle" : "information-circle-outline"} size={20} />
          ),
        }}
      />
      <Tabs.Screen
        name="attachments"
        options={{
          title: "Attachments",
          tabBarIcon: ({ color, focused }) => (
            <Ionicons color={color} name={focused ? "attach" : "attach-outline"} size={20} />
          ),
        }}
      />
    </Tabs>
  );
};

export default IssueTabsLayout;
