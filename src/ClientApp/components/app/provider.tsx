import "react-native-reanimated";

import { DarkTheme, DefaultTheme } from "@react-navigation/native";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import React, { ReactNode } from "react";

import { AuthProvider } from "@/store/auth_context";
import { ThemeProvider } from "@react-navigation/native";
import { queryConfig } from "@/lib/react-query";
import { useColorScheme } from "@/hooks/useColorScheme";
import { useReactQueryDevTools } from "@dev-plugins/react-query";

type AppProviderProps = {
  children: ReactNode;
};

const AppProvider = ({ children }: AppProviderProps) => {
  const colorScheme = useColorScheme();
  const [queryClient] = React.useState(
    () =>
      new QueryClient({
        defaultOptions: queryConfig,
      })
  );
  useReactQueryDevTools(queryClient);

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider value={colorScheme === "dark" ? DarkTheme : DefaultTheme}>
        <AuthProvider>{children}</AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
};
export default AppProvider;
