import { Redirect, Slot, Stack, router } from "expo-router";

import AppProvider from "@/components/app/provider";

const Root = () => {
  return (
    <AppProvider>
      <Slot />
    </AppProvider>
  );
};

export default Root;
