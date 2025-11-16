import { Slot, useNavigationContainerRef } from "expo-router";

import AppProvider from "@/components/app/provider";
import { Stack } from "expo-router";
import { useAuth } from "@/store/auth_context";
import { useReactNavigationDevTools } from "@dev-plugins/react-navigation";

const RootLayout = () => {
  const navigationRef = useNavigationContainerRef();

  useReactNavigationDevTools(navigationRef);
  return (
    <AppProvider>
      <Root />
    </AppProvider>
  );
};

const Root = () => {
  const { isAuthenticated } = useAuth();

  return (
    <Stack>
      <Stack.Protected guard={isAuthenticated}>
        <Stack.Screen name="(app)" options={{ headerShown: false }} />
      </Stack.Protected>
      <Stack.Protected guard={!isAuthenticated}>
        <Stack.Screen name="SignIn" options={{ headerShown: false }} />
        <Stack.Screen name="SignUp" options={{ headerShown: false }} />
        <Stack.Screen
          name="VerifyEmail"
          options={{ presentation: "modal", gestureEnabled: false }}
        />
      </Stack.Protected>
    </Stack>
  );
};

export default RootLayout;
