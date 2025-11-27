import { Link, Stack, router } from "expo-router";
import { Pressable, StyleSheet, Text, View } from "react-native";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function NotFoundScreen() {
  const colorPalette = useColorPalette();
  return (
    <>
      <Stack.Screen options={{ title: "Oops!" }} />
      <View style={styles.container}>
        <Text style={{ color: colorPalette.text }}>This screen doesn't exist.</Text>
        <Pressable onPress={() => router.replace("/SignIn")}>
          <Text style={{ color: colorPalette.text }}>Go to home screen!</Text>
        </Pressable>
      </View>
    </>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    padding: 20,
  },
  link: {
    marginTop: 15,
    paddingVertical: 15,
  },
});
