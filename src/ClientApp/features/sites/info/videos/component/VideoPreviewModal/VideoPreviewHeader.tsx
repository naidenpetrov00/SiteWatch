import { MaterialIcons } from "@expo/vector-icons";
import { Pressable, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import { videoPreviewModalStyles } from "./VideoPreviewModal.styles";

interface IVideoPreviewHeader {
  onClose: () => void;
}

const VideoPreviewHeader = ({ onClose }: IVideoPreviewHeader) => {
  const colorPalette = useColorPalette();

  return (
    <View style={videoPreviewModalStyles.header}>
      <Pressable
        accessibilityLabel="Close video preview"
        accessibilityRole="button"
        onPress={onClose}
        style={({ pressed }) => [
          videoPreviewModalStyles.closeButton,
          { backgroundColor: colorPalette.primary },
          pressed ? videoPreviewModalStyles.closeButtonPressed : null,
        ]}
      >
        <MaterialIcons
          color={colorPalette.contrastText}
          name="close"
          size={24}
        />
      </Pressable>
    </View>
  );
};

export default VideoPreviewHeader;
