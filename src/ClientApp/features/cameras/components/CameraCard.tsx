import { Image, Pressable, StyleSheet, Text, View } from "react-native";

import { Camera } from "../api/types";
import React from "react";
import { useColorPalette } from "@/hooks/useColorPalette";

type Props = {
  camera: Camera;
  onPress?: () => void;
};

const CameraCard: React.FC<Props> = ({ camera, onPress }) => {
  const colorPalette = useColorPalette();

  return (
    <Pressable
      onPress={onPress}
      style={[styles.card, { backgroundColor: colorPalette.background }]}
    >
      <View style={styles.header}>
        <Text style={[styles.title, { color: colorPalette.text }]}>
          {camera.name || `Camera ${camera.id}`}
        </Text>
      </View>

      <View style={styles.snapshotWrapper}>
        <Image style={styles.snapshot} resizeMode="cover" />
      </View>
    </Pressable>
  );
};

const styles = StyleSheet.create({
  card: {
    borderRadius: 12,
    overflow: "hidden",
    marginBottom: 16,
    elevation: 2,
  },
  header: {
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  title: {
    fontWeight: "600",
    fontSize: 14,
  },
  snapshotWrapper: {
    width: "100%",
    aspectRatio: 16 / 9, // "window" feel
    backgroundColor: "#111",
  },
  snapshot: {
    width: "100%",
    height: "100%",
  },
});

export default CameraCard;
