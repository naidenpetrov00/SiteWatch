import { Stack } from "expo-router";

const IssuesLayout = () => (
  <Stack>
    <Stack.Screen name="index" options={{ title: "Issues" }} />
    <Stack.Screen name="[issueId]" options={{ title: "Issue details" }} />
  </Stack>
);

export default IssuesLayout;
