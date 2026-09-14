import type { ReactNode } from "react";
import { Pressable, StyleSheet, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";

export type ConnectedTab = {
  label: string;
  value: string;
};

type ConnectedTabsProps = {
  children: ReactNode;
  onValueChange: (value: string) => void;
  tabs: readonly ConnectedTab[];
  value: string;
};

const ConnectedTabs = ({ children, onValueChange, tabs, value }: ConnectedTabsProps) => {
  const colorPalette = useColorPalette();
  return <View>
    <View style={styles.tabBar}>
      {tabs.map((tab, index) => {
        const isActive = tab.value === value;
        return <Pressable key={tab.value} accessibilityRole="tab" accessibilityState={{ selected: isActive }} onPress={() => onValueChange(tab.value)} style={({ pressed }) => [styles.tab, index === 0 ? styles.firstTab : styles.lastTab, isActive ? styles.activeTab : styles.inactiveTab, { backgroundColor: colorPalette.background, borderColor: colorPalette.primary, opacity: pressed ? 0.78 : 1 }]}><Text style={[styles.tabLabel, isActive ? styles.activeTabLabel : null, { color: isActive ? colorPalette.primary : colorPalette.secondary }]}>{tab.label}</Text></Pressable>;
      })}
    </View>
    <View style={[styles.panel, { backgroundColor: colorPalette.background, borderColor: colorPalette.primary }]}>{children}</View>
  </View>;
};

const styles = StyleSheet.create({
  tabBar: { alignItems: "flex-end", flexDirection: "row" },
  tab: { alignItems: "center", borderWidth: 1, flex: 1, paddingHorizontal: 12, paddingVertical: 10 },
  firstTab: { borderTopLeftRadius: 14 },
  lastTab: { borderTopRightRadius: 14 },
  activeTab: { borderBottomWidth: 0, zIndex: 1 },
  inactiveTab: { borderBottomWidth: 1, zIndex: 0 },
  tabLabel: { fontSize: 14, fontWeight: "700" },
  activeTabLabel: { fontSize: 16 },
  panel: { borderBottomLeftRadius: 14, borderBottomRightRadius: 14, borderWidth: 1, borderTopWidth: 0, gap: 16, padding: 16 },
});

export default ConnectedTabs;
