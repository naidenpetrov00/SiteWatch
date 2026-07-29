import * as ScreenOrientation from "expo-screen-orientation";

import { useNavigationContainerRef } from "expo-router";

import AppProvider from "@/components/app/provider";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import { Stack } from "expo-router";
import { useAuth } from "@/store/auth_context";
import { useEffect } from "react";
import { useReactNavigationDevTools } from "@dev-plugins/react-navigation";
import { ACCESS_POLICIES } from "@/types/authorization";

const RootLayout = () => {
  const navigationRef = useNavigationContainerRef();

  useReactNavigationDevTools(navigationRef);
  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <AppProvider>
        <Root />
      </AppProvider>
    </GestureHandlerRootView>
  );
};

const Root = () => {
  const { isAuthenticated, hasAnyRole } = useAuth();
  const canAccessCurrentApp = hasAnyRole(ACCESS_POLICIES.currentApp);
  useEffect(() => {
    ScreenOrientation.lockAsync(ScreenOrientation.OrientationLock.PORTRAIT_UP);
  }, []);

  return (
    <Stack>
      <Stack.Protected guard={isAuthenticated && canAccessCurrentApp}>
        <Stack.Screen name="(app)" options={{ headerShown: false }} />
      </Stack.Protected>
      <Stack.Protected guard={isAuthenticated && !canAccessCurrentApp}>
        <Stack.Screen name="AccessDenied" options={{ headerShown: false }} />
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
