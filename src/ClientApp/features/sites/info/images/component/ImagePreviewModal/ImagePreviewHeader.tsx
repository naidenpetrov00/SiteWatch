import { Pressable, View } from "react-native";
import { useCallback } from "react";

import { MaterialIcons } from "@expo/vector-icons";
import { imagePreviewModalStyles } from "./ImagePreviewModal.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IImagePreviewHeader {
  onClose: () => void;
}

const ImagePreviewHeader = ({ onClose }: IImagePreviewHeader) => {
  const colorPalette = useColorPalette();
  const handleDownloadPress = useCallback(() => {}, []);

  return (
    <View style={imagePreviewModalStyles.header}>
      <View style={imagePreviewModalStyles.headerActions}>
        <Pressable
          accessibilityLabel="Download image"
          accessibilityRole="button"
          onPress={handleDownloadPress}
          style={({ pressed }) => [
            imagePreviewModalStyles.actionButton,
            { backgroundColor: colorPalette.primary },
            pressed ? imagePreviewModalStyles.actionButtonPressed : null,
          ]}
        >
          <MaterialIcons
            color={colorPalette.contrastText}
            name="file-download"
            size={24}
          />
        </Pressable>
        <Pressable
          accessibilityLabel="Close image preview"
          accessibilityRole="button"
          onPress={onClose}
          style={({ pressed }) => [
            imagePreviewModalStyles.actionButton,
            { backgroundColor: colorPalette.primary },
            pressed ? imagePreviewModalStyles.actionButtonPressed : null,
          ]}
        >
          <MaterialIcons
            color={colorPalette.contrastText}
            name="close"
            size={24}
          />
        </Pressable>
      </View>
    </View>
  );
};

export default ImagePreviewHeader;
