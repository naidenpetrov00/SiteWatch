import { Pressable, Text, View } from "react-native";

import ComingSoon from "@/components/ui/ComingSoon";
import { useAuth } from "@/store/auth_context";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function Index() {
  const colorPalette = useColorPalette();

  const { logout } = useAuth();
  return (
    <View style={{ flex: 1, justifyContent: "center" }}>
      <ComingSoon />
      <Pressable
        style={({ pressed }) => ({
          alignSelf: "center",
          backgroundColor: colorPalette.primary,
          paddingHorizontal: 18,
          paddingVertical: 10,
          borderRadius: 10,
          opacity: pressed ? 0.75 : 1,
        })}
        onPress={logout}
      >
        <Text style={{ color: colorPalette.background, fontWeight: "700" }}>
          Sign Out
        </Text>
      </Pressable>
    </View>
  );
}
