import { Alert, Modal, Pressable, ScrollView, Text, View } from "react-native";

import { Camera } from "../../api/models";
import { Site } from "@/features/sites/api/types";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useMoveCameraToSite } from "../../api/manage-cameras";
import cameraManagementStyles from "./camera-management.styles";

type CameraMoveSheetProps = {
  camera: Camera | null;
  sourceSiteId: string;
  sites: readonly Site[];
  visible: boolean;
  onClose: () => void;
};

const errorMessage = (error: unknown) =>
  error instanceof Error && error.message.trim()
    ? error.message
    : "Please choose a site and try again.";

const CameraMoveSheet = ({
  camera,
  sourceSiteId,
  sites,
  visible,
  onClose,
}: CameraMoveSheetProps) => {
  const colorPalette = useColorPalette();
  const moveCamera = useMoveCameraToSite(sourceSiteId);
  const destinations = sites.filter((site) => site.id !== sourceSiteId);

  const move = async (siteId: string) => {
    if (!camera) return;

    try {
      await moveCamera.mutateAsync({ cameraId: camera.id, siteId });
      onClose();
    } catch (error) {
      Alert.alert("Unable to move camera", errorMessage(error));
    }
  };

  return (
    <Modal
      animationType="slide"
      onRequestClose={onClose}
      presentationStyle="formSheet"
      visible={visible}
    >
      <ScrollView
        contentContainerStyle={cameraManagementStyles.content}
        contentInsetAdjustmentBehavior="automatic"
        style={{ backgroundColor: colorPalette.background }}
      >
        <Text selectable style={{ color: colorPalette.text, fontSize: 22, fontWeight: "700" }}>
          Move Camera
        </Text>
        <Text selectable style={{ color: colorPalette.secondary }}>
          Choose the new site for {camera?.name || "this camera"}.
        </Text>
        {destinations.length ? destinations.map((site) => (
          <Pressable
            key={site.id}
            accessibilityLabel={`Move to ${site.name}`}
            accessibilityRole="button"
            accessibilityState={{ busy: moveCamera.isPending }}
            disabled={moveCamera.isPending}
            onPress={() => void move(site.id)}
            style={({ pressed }) => [
              cameraManagementStyles.siteItem,
              {
                backgroundColor: colorPalette.background,
                borderColor: colorPalette.primary,
                opacity: moveCamera.isPending ? 0.55 : pressed ? 0.78 : 1,
              },
            ]}
          >
            <Text selectable style={{ color: colorPalette.text, fontSize: 16, fontWeight: "600" }}>
              {site.name}
            </Text>
            <Text selectable style={{ color: colorPalette.secondary }}>
              {site.address}
            </Text>
          </Pressable>
        )) : (
          <Text selectable style={{ color: colorPalette.secondary }}>
            There are no other sites available to move this camera to.
          </Text>
        )}
      </ScrollView>
      <View style={[cameraManagementStyles.footer, { backgroundColor: colorPalette.background }]}>
        <Pressable
          onPress={onClose}
          style={({ pressed }) => [
            cameraManagementStyles.button,
            { backgroundColor: colorPalette.secondary, opacity: pressed ? 0.78 : 1 },
          ]}
        >
          <Text style={{ color: colorPalette.background, fontWeight: "600" }}>Cancel</Text>
        </Pressable>
      </View>
    </Modal>
  );
};

export default CameraMoveSheet;
