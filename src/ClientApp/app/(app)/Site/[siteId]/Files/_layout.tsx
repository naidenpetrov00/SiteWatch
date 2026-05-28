import { Stack } from "expo-router";

const FilesLayout = () => {
  return (
    <Stack>
      <Stack.Screen name="index" options={{ title: "Files" }} />
    </Stack>
  );
};

export default FilesLayout;
