import { Text, View } from "react-native";

import { Redirect } from "expo-router";
import { useAuth } from "@/store/auth_context";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function Index() {
  const colorPalette = useColorPalette();

  const { logout } = useAuth();
  return (
    <View style={{ flex: 1, justifyContent: "center", alignItems: "center" }}>
      <Text
        style={{ color: colorPalette.text }}
        onPress={() => {
          logout();
          return <Redirect href="/" />;
        }}
      >
        Sign Out
      </Text>
    </View>
  );
}
