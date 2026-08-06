import { Pressable, StyleSheet, Text, View } from "react-native";

import { useAuth } from "@/store/auth_context";
import { useColorPalette } from "@/hooks/useColorPalette";

const AccessDenied = () => {
  const { logout } = useAuth();
  const colorPalette = useColorPalette();

  return (
    <View style={styles.container}>
      <Text style={[styles.title, { color: colorPalette.text }]}>Access denied</Text>
      <Text style={[styles.message, { color: colorPalette.text }]}>
        Your account does not have access to the current SiteWatch mobile screens.
      </Text>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel="Sign out"
        onPress={logout}
        style={({ pressed }) => [
          styles.button,
          {
            backgroundColor: colorPalette.primary,
            opacity: pressed ? 0.75 : 1,
          },
        ]}
      >
        <Text style={[styles.buttonText, { color: colorPalette.background }]}>Sign Out</Text>
      </Pressable>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    padding: 24,
    gap: 16,
  },
  title: {
    fontSize: 28,
    fontWeight: "700",
  },
  message: {
    maxWidth: 420,
    textAlign: "center",
    fontSize: 16,
    lineHeight: 24,
  },
  button: {
    borderRadius: 10,
    paddingHorizontal: 18,
    paddingVertical: 10,
  },
  buttonText: {
    fontWeight: "700",
  },
});

export default AccessDenied;
