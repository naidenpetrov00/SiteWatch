import {
  FlatList,
  RefreshControl,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";

import CameraCard from "@/features/cameras/components/CameraCard/CameraCard";
import LoadingState from "@/components/app/LoadingState";
import React from "react";
import cameraStyles from "@/features/cameras/components/Cameras.styles";
import { useCamerasBySite } from "@/features/cameras/api/get-cameras-by-site";
import { useColorPalette } from "@/hooks/useColorPalette";

const Cameras = () => {
  const { siteId } = useLocalSearchParams<{ siteId: string }>();
  const router = useRouter();
  const colorPalette = useColorPalette();

  const {
    data: cameras,
    isLoading,
    isRefetching,
    isStale,
    refetch,
  } = useCamerasBySite({ siteId });

  const handleAddCamera = () => {
    console.log("Add camera tapped");
  };

  if (isLoading) {
    return <LoadingState label="Loading cameras..." />;
  }

  return (
    <View
      style={[
        cameraStyles.container,
        { backgroundColor: colorPalette.background },
      ]}
    >
      <FlatList
        data={cameras ?? []}
        keyExtractor={(item) => item.id}
        refreshControl={
          <RefreshControl
            refreshing={isRefetching}
            onRefresh={refetch}
            tintColor={colorPalette.primary}
            colors={[colorPalette.primary]}
          />
        }
        contentContainerStyle={[
          cameraStyles.listContent,
          {
            backgroundColor: colorPalette.background,
            justifyContent: cameras?.length ? undefined : "center",
          },
        ]}
        ListEmptyComponent={
          <View style={cameraStyles.emptyState}>
            <Text style={{ color: colorPalette.text, fontSize: 16 }}>
              No cameras at this Site
            </Text>
            <TouchableOpacity
              style={[
                cameraStyles.addButton,
                { backgroundColor: colorPalette.primary },
              ]}
              onPress={handleAddCamera}
            >
              <Text
                style={{ color: colorPalette.background, fontWeight: "600" }}
              >
                Add camera
              </Text>
            </TouchableOpacity>
          </View>
        }
        renderItem={({ item }) => (
          <CameraCard
            camera={item}
            onPress={() =>
              router.push({
                pathname: "/Camera/[cameraId]",
                params: { cameraId: item.id, siteId },
              })
            }
          />
        )}
      />
    </View>
  );
};

export default Cameras;
