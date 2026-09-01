import { Stack } from "expo-router";

const IssuesLayout = () => (
  <Stack>
    <Stack.Screen name="index" options={{ title: "Issues" }} />
  </Stack>
);

export default IssuesLayout;
