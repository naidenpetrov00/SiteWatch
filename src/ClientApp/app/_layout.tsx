import { Modal, Text } from "react-native";
import { Slot, Stack } from "expo-router";

import AppProvider from "@/components/app/provider";

const RootLayout = () => {
  const isAuthenticated = false;

  return (
    <AppProvider>
      <Stack>
        <Stack.Protected guard={isAuthenticated}>
          <Stack.Screen name="(app)" options={{ headerShown: false }} />
        </Stack.Protected>
        <Stack.Protected guard={!isAuthenticated}>
          <Stack.Screen name="SignUp" options={{ headerShown: false }} />
          <Stack.Screen name="SignIn" options={{ headerShown: false }} />
          <Stack.Screen
            name="VerifyEmail"
            options={{ presentation: "modal", gestureEnabled: false }}
          />
        </Stack.Protected>
      </Stack>
    </AppProvider>
  );
};

export default RootLayout;
