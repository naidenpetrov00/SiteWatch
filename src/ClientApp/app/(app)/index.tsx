import { Text, View } from "react-native";

import { Redirect } from "expo-router";

export default function Index() {
  return (
    <View style={{ flex: 1, justifyContent: "center", alignItems: "center" }}>
      <Text
        onPress={() => {
          // The `app/(app)/_layout.tsx` will redirect to the sign-in screen.
          return <Redirect href="/" />;
        }}
      >
        Sign Out
      </Text>
    </View>
  );
}
