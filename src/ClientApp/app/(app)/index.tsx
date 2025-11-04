import { Text, View } from "react-native";

import { Redirect } from "expo-router";
import { useAuth } from "@/store/auth_context";

export default function Index() {
  const { logout } = useAuth();
  return (
    <View style={{ flex: 1, justifyContent: "center", alignItems: "center" }}>
      <Text
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
