import { Alert, TouchableOpacity, View } from "react-native";
import * as MediaLibrary from "expo-media-library";

import { Camera } from "@/features/cameras/api/models";
import { ChannelType } from "../../types";
import { MaterialIcons } from "@expo/vector-icons";
import React from "react";
import cameraManagmentCardStyles from "@/features/cameras/components/CameraHdCard/CameraManagmentCard.styles";
import cardStyles from "@/components/ui/Card/Card.styles";
import { saveSnapshot } from "../../utils";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useGetCameraSnapshot } from "@/features/cameras/api/get-camera-snapshot";

type CameraManagmentCardProps = {
  channel: ChannelType;
  setChannel: React.Dispatch<React.SetStateAction<ChannelType>>;
  camera: Camera;
  onToggleRecording?: () => void;
};

const CameraManagmentCard = ({
  channel,
  setChannel,
  camera,
  onToggleRecording,
}: CameraManagmentCardProps) => {
  const colorPalette = useColorPalette();

  const { isFetching, refetch } = useGetCameraSnapshot({
    data: {
      ipAddress: camera?.ipAddress,
      username: camera?.username,
      password: camera?.password,
    },
  });

  const handleChannelChange = () => {
    if (channel == ChannelType.Sub) setChannel(ChannelType.Main);
    else setChannel(ChannelType.Sub);
  };

  const handleSnapshotPress = async () => {
    if (!camera || isFetching) return;

    try {
      const perm = await MediaLibrary.requestPermissionsAsync();
      if (perm.status !== "granted") {
        Alert.alert(
          "Permission required",
          "Allow photo access to save snapshots.",
        );
        return;
      }

      const result = await refetch();
      const dataUrl = result.data;

      if (!dataUrl) {
        console.warn("No snapshot returned.");
        return;
      }

      await saveSnapshot(dataUrl, camera.id);
      Alert.alert("Saved", "Snapshot saved to your device.");
    } catch (e) {
      throw e;
    }
  };

  return (
    <View
      style={[
        cardStyles.card,
        cameraManagmentCardStyles.card,
        {
          backgroundColor: colorPalette.background,
          borderColor: colorPalette.secondary,
        },
      ]}
    >
      <View style={cameraManagmentCardStyles.buttonsRow}>
        <TouchableOpacity
          onPress={handleSnapshotPress}
          disabled={!camera || isFetching}
          style={[
            cameraManagmentCardStyles.iconButton,
            { backgroundColor: colorPalette.primary },
          ]}
        >
          <MaterialIcons
            name="photo-camera"
            size={18}
            color={colorPalette.background}
          />
        </TouchableOpacity>
        <TouchableOpacity
          style={[
            cameraManagmentCardStyles.iconButton,
            { backgroundColor: colorPalette.primary },
          ]}
          onPress={onToggleRecording}
        >
          <MaterialIcons
            name="videocam"
            size={18}
            color={colorPalette.background}
          />
        </TouchableOpacity>
        <TouchableOpacity
          style={[
            cameraManagmentCardStyles.iconButton,
            { backgroundColor: colorPalette.primary },
          ]}
          onPress={handleChannelChange}
        >
          <MaterialIcons
            name={channel == ChannelType.Sub ? "sd" : "hd"}
            size={20}
            color={colorPalette.background}
          />
        </TouchableOpacity>
        <TouchableOpacity
          disabled={true}
          style={[
            cameraManagmentCardStyles.iconButton,
            { backgroundColor: colorPalette.primary },
          ]}
        >
          <MaterialIcons
            name="expand-circle-down"
            size={20}
            color={colorPalette.background}
          />
        </TouchableOpacity>
      </View>
    </View>
  );
};

export default CameraManagmentCard;
