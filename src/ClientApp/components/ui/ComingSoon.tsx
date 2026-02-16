import { StyleSheet, Text, View } from "react-native";

import React from "react";
import { useColorPalette } from "@/hooks/useColorPalette";

interface ComingSoonProps {
  title?: string;
  description?: string;
  badgeText?: string;
}

const ComingSoon: React.FC<ComingSoonProps> = ({
  title = "Coming Soon",
  description = "This section is in progress and will be available soon.",
  badgeText = "IN DEVELOPMENT",
}) => {
  const colorPalette = useColorPalette();

  return (
    <View style={styles.container}>
      <View
        style={[
          styles.card,
          {
            backgroundColor: colorPalette.background,
            borderColor: colorPalette.primary,
          },
        ]}
      >
        <View
          style={[
            styles.badge,
            { backgroundColor: colorPalette.primary + "1A" },
          ]}
        >
          <Text style={[styles.badgeText, { color: colorPalette.primary }]}>
            {badgeText}
          </Text>
        </View>

        <Text style={[styles.title, { color: colorPalette.text }]}>{title}</Text>
        <Text style={[styles.description, { color: colorPalette.secondary }]}>
          {description}
        </Text>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    // flex: 1,
    justifyContent: "center",
    alignItems: "center",
    paddingHorizontal: 32,
    marginBottom:24
  },
  card: {
    width: "100%",
    maxWidth: 440,
    borderWidth: 1,
    borderRadius: 20,
    paddingHorizontal: 20,
    paddingVertical: 24,
    alignItems: "center",
  },
  badge: {
    borderRadius: 999,
    paddingHorizontal: 10,
    paddingVertical: 6,
    marginBottom: 12,
  },
  badgeText: {
    fontSize: 11,
    fontWeight: "700",
    letterSpacing: 0.6,
  },
  title: {
    fontSize: 24,
    fontWeight: "800",
    marginBottom: 8,
    textAlign: "center",
  },
  description: {
    fontSize: 14,
    textAlign: "center",
    lineHeight: 20,
  },
});

export default ComingSoon;
