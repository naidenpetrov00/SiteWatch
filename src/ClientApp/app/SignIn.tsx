import { Button, Text, View } from "react-native";

import React from "react";
import { router } from "expo-router";

export default function SignIn() {
  return (
    <View
      style={{
        flex: 1,
        alignItems: "center",
        justifyContent: "center",
        gap: 16,
      }}
    >
      <Text style={{ fontSize: 22, fontWeight: "600" }}>Register</Text>
      {/* Put your inputs here */}
      <Button
        title="Create account"
        onPress={() => {
          router.push("/SignUp");
        }}
      />
      <Button title="Back to Login" onPress={() => router.back()} />
    </View>
  );
}
