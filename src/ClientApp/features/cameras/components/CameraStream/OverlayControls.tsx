import React from "react";
import { TouchableOpacity, View } from "react-native";

import { MaterialIcons } from "@expo/vector-icons";
import { overlayControlsStyles } from "./OverlayControls.styles";

interface OverlayControlsProps {
  isMuted: boolean;
  isLandscape: boolean;
  visible: boolean;
  onInteraction: () => void;
  setIsMuted: React.Dispatch<React.SetStateAction<boolean>>;
  onToggleFullscreen: () => Promise<void>;
}

const OverlayControls: React.FC<OverlayControlsProps> = ({
  isMuted,
  isLandscape,
  visible,
  onInteraction,
  setIsMuted,
  onToggleFullscreen,
}) => {
  const toggleMute = () => {
    setIsMuted((prev) => !prev);
    onInteraction();
  };

  return (
    <View
      style={[
        overlayControlsStyles.overlay,
        !visible && overlayControlsStyles.hidden,
      ]}
      pointerEvents={visible ? "auto" : "none"}
    >
      <TouchableOpacity
        style={overlayControlsStyles.controlButton}
        onPress={toggleMute}
      >
        <MaterialIcons
          name={isMuted ? "volume-off" : "volume-up"}
          size={16}
          color="#fff"
        />
      </TouchableOpacity>
      <TouchableOpacity
        style={overlayControlsStyles.controlButton}
        onPress={() => {
          void onToggleFullscreen();
          onInteraction();
        }}
      >
        <MaterialIcons
          name={isLandscape ? "fullscreen-exit" : "fullscreen"}
          size={16}
          color="#fff"
        />
      </TouchableOpacity>
    </View>
  );
};

export default OverlayControls;
