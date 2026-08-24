import {
  Alert,
  FlatList,
  Pressable,
  RefreshControl,
  Text,
  View,
} from "react-native";

import CameraCard from "@/features/cameras/components/CameraCard/CameraCard";
import LoadingState from "@/components/app/LoadingState";
import React from "react";
import cameraStyles from "@/features/cameras/components/Cameras.styles";
import { useCamerasBySite } from "@/features/cameras/api/get-cameras-by-site";
import { Camera } from "@/features/cameras/api/models";
import { useDeleteCamera } from "@/features/cameras/api/manage-cameras";
import CameraAddSheet from "@/features/cameras/components/CameraManagement/camera-add-sheet";
import CameraMoveSheet from "@/features/cameras/components/CameraManagement/camera-move-sheet";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetSearchParams from "@/hooks/useGetSearchParams";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";
import { ACCESS_POLICIES } from "@/types/authorization";
import { useAuth } from "@/store/auth_context";
import { useRouter } from "expo-router";
import { useCallback, useState } from "react";

const Cameras = () => {
  const params = useGetSearchParams<{
    siteId?: string;
    siteName?: string;
  }>();
  const siteId = params.siteId;
  const router = useRouter();
  const colorPalette = useColorPalette();
  const { hasAnyRole } = useAuth();
  const canManageCameras = hasAnyRole(ACCESS_POLICIES.cameraManagement);
  const [isAddSheetOpen, setIsAddSheetOpen] = useState(false);
  const [cameraToMove, setCameraToMove] = useState<Camera | null>(null);

  const {
    data: cameras,
    isLoading,
    isRefetching,
    refetch,
  } = useCamerasBySite({ siteId });
  const { data: sites = [] } = useGetSitesByUserId();
  const deleteCamera = useDeleteCamera(siteId ?? "");

  const handleAddCamera = useCallback(() => setIsAddSheetOpen(true), []);

  const handleDeleteCamera = useCallback((camera: Camera) => {
    Alert.alert(
      "Delete camera?",
      `Delete ${camera.name || "this camera"}? This cannot be undone.`,
      [
        { text: "Cancel", style: "cancel" },
        {
          text: "Delete",
          style: "destructive",
          onPress: () => {
            void deleteCamera.mutateAsync(camera.id).catch((error: unknown) => {
              const message = error instanceof Error && error.message.trim()
                ? error.message
                : "Please try again.";
              Alert.alert("Unable to delete camera", message);
            });
          },
        },
      ],
    );
  }, [deleteCamera]);

  const handleManageCamera = useCallback((camera: Camera) => {
    Alert.alert(
      `Manage ${camera.name || "camera"}`,
      undefined,
      [
        { text: "Move", onPress: () => setCameraToMove(camera) },
        { text: "Delete", style: "destructive", onPress: () => handleDeleteCamera(camera) },
        { text: "Cancel", style: "cancel" },
      ],
    );
  }, [handleDeleteCamera]);

  const renderCamera = useCallback(({ item }: { item: Camera }) => (
    <CameraCard
      camera={item}
      onManage={canManageCameras ? () => handleManageCamera(item) : undefined}
      onPress={() =>
        router.push({
          pathname: "/Camera/[cameraId]",
          params: { cameraId: item.id, siteId },
        })
      }
    />
  ), [canManageCameras, handleManageCamera, router, siteId]);

  if (isLoading) {
    return <LoadingState label="Loading cameras..." />;
  }

  if (!siteId) {
    return <LoadingState label="Loading site..." />;
  }

  return (
    <View
      style={[
        cameraStyles.container,
        { backgroundColor: colorPalette.background },
      ]}
    >
      <FlatList<Camera>
        contentInsetAdjustmentBehavior="automatic"
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
            <Text selectable style={{ color: colorPalette.text, fontSize: 16 }}>
              No cameras at this Site
            </Text>
            {canManageCameras ? (
              <Pressable
                accessibilityLabel="Add camera"
                accessibilityRole="button"
                onPress={handleAddCamera}
                style={({ pressed }) => [
                  cameraStyles.addButton,
                  { backgroundColor: colorPalette.primary, opacity: pressed ? 0.78 : 1 },
                ]}
              >
                <Text style={{ color: colorPalette.background, fontWeight: "600" }}>
                  Add camera
                </Text>
              </Pressable>
            ) : null}
          </View>
        }
        renderItem={renderCamera}
      />
      {canManageCameras ? (
        <>
          <CameraAddSheet
            onClose={() => setIsAddSheetOpen(false)}
            siteId={siteId}
            visible={isAddSheetOpen}
          />
          <CameraMoveSheet
            camera={cameraToMove}
            onClose={() => setCameraToMove(null)}
            sites={sites}
            sourceSiteId={siteId}
            visible={cameraToMove !== null}
          />
        </>
      ) : null}
    </View>
  );
};

export default Cameras;
