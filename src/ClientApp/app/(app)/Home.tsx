import { Button, Text, View } from "react-native";

import React from "react";
import { router } from "expo-router";

export default function Home() {
  return (
    <View
      style={{
        flex: 1,
        alignItems: "center",
        justifyContent: "center",
        gap: 16,
      }}
    >
      <Text style={{ fontSize: 22, fontWeight: "600" }}>Welcome Home</Text>
    </View>
  );
}
