import { Pressable, TouchableOpacity, View } from "react-native";

import { ChannelType } from "../../types";
import { MaterialIcons } from "@expo/vector-icons";
import React from "react";
import cameraManagmentCardStyles from "@/features/cameras/components/CameraHdCard/CameraHdCard.styles";
import cardStyles from "@/components/ui/Card/Card.styles";
import { set } from "react-hook-form";
import { useColorPalette } from "@/hooks/useColorPalette";

type CameraManagmentCardProps = {
  channel: ChannelType;
  setChannel: React.Dispatch<React.SetStateAction<ChannelType>>;
};

const CameraManagmentCard = ({
  channel,
  setChannel,
}: CameraManagmentCardProps) => {
  const colorPalette = useColorPalette();

  const handleChannelChange = () => {
    if (channel == ChannelType.Sub) setChannel(ChannelType.Main);
    else setChannel(ChannelType.Sub);
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
