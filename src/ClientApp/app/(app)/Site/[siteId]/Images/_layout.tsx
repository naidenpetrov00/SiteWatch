import { Stack } from "expo-router";

const ImagesLayout = () => {
  return (
    <Stack>
      <Stack.Screen name="index" options={{ title: "Images" }} />
    </Stack>
  );
};

export default ImagesLayout;
