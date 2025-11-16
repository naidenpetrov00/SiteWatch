import { FlatList, Text, View } from "react-native";

import CameraCard from "@/features/cameras/components/CameraCard";
// app/sites/[siteId].tsx
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useLocalSearchParams } from "expo-router";

const Info = () => {
  const { siteId } = useLocalSearchParams<{ siteId: string }>();
  console.log(siteId);
  const colorPalette = useColorPalette();

  const cameras = mockCameras;

  //   if (isLoading) {
  //     return (
  //       <SafeAreaView
  //         style={{ flex: 1, justifyContent: "center", alignItems: "center" }}
  //       >
  //         <Text style={{ color: colorPalette.text }}>Loading cameras...</Text>
  //       </SafeAreaView>
  //     );
  //   }

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: colorPalette.background }}>
      <FlatList
        data={cameras}
        keyExtractor={(item) => item.id}
        contentContainerStyle={{ padding: 16 }}
        renderItem={({ item }) => (
          <CameraCard
            camera={item}
            onPress={() => {
              // later: open full-screen live view, etc.
              console.log("Pressed camera:", item.id);
            }}
          />
        )}
      />
    </SafeAreaView>
  );
};

export default Info;
export const mockCameras = [
  {
    id: "cam-1",
    name: "Entrance Camera",
    snapshotUrl:
      "https://placehold.co/800x450?text=Entrance+Camera&font=roboto",
  },
  {
    id: "cam-2",
    name: "Office Hallway",
    snapshotUrl: "https://placehold.co/800x450?text=Office+Hallway&font=roboto",
  },
  {
    id: "cam-3",
    name: "Warehouse Corner",
    snapshotUrl:
      "https://placehold.co/800x450?text=Warehouse+Corner&font=roboto",
  },
  {
    id: "cam-4",
    name: "Parking Lot",
    snapshotUrl: "https://placehold.co/800x450?text=Parking+Lot&font=roboto",
  },
  {
    id: "cam-5",
    name: "Back Door",
    snapshotUrl: "https://placehold.co/800x450?text=Back+Door&font=roboto",
  },
];
