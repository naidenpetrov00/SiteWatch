import { Pressable, StyleSheet, Text, View } from "react-native";

// components/SiteCard/SiteCard.tsx
import React from "react";
import { Site } from "../api/types";
import { useColorPalette } from "@/hooks/useColorPalette";

interface SiteCardProps {
  site: Site;
  onPress: (site: Site) => void;
}

const SiteCard = ({ site, onPress }: SiteCardProps) => {
  const colorPalette = useColorPalette();

  return (
    <Pressable
      onPress={() => onPress(site)}
      style={({ pressed }) => [
        styles.card,
        {
          backgroundColor: colorPalette.background,
          borderColor: colorPalette.primary,
        },
        pressed && { opacity: 0.9, transform: [{ scale: 0.98 }] },
      ]}
    >
      <View style={styles.content}>
        <Text style={[styles.title, { color: colorPalette.text }]}>
          {site.name}
        </Text>
        <Text style={[styles.address, { color: colorPalette.text }]}>
          {site.address}
        </Text>
      </View>
    </Pressable>
  );
};

const styles = StyleSheet.create({
  card: {
    borderWidth: 1,
    borderRadius: 16,
    paddingVertical: 16,
    paddingHorizontal: 18,
    marginBottom: 12,
    shadowColor: "#000",
    shadowOpacity: 0.08,
    shadowRadius: 6,
    shadowOffset: { width: 0, height: 2 },
    elevation: 2,
  },
  content: {
    gap: 4,
  },
  title: {
    fontSize: 18,
    fontWeight: "600",
  },
  address: {
    fontSize: 14,
    opacity: 0.8,
  },
});

export default SiteCard;
