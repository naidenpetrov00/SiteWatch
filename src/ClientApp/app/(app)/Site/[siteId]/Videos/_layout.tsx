import { Stack } from "expo-router";

const VideosLayout = () => {
  return (
    <Stack>
      <Stack.Screen name="index" options={{ title: "Videos" }} />
    </Stack>
  );
};

export default VideosLayout;