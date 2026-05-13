import { Modal, StatusBar } from "react-native";

import { GestureHandlerRootView } from "react-native-gesture-handler";
import ImagePreviewHeader from "./ImagePreviewHeader";
import { SafeAreaView } from "react-native-safe-area-context";
import ZoomablePreviewImage from "./ZoomablePreviewImage";
import { imagePreviewModalStyles } from "./ImagePreviewModal.styles";
import { useGetSiteImageFull } from "../../hooks/useGetSiteImageFull";

interface IImagePreviewModal {
  imageId: string;
  thumbnailUri: string;
  visible: boolean;
  onClose: () => void;
  enableSwipeToClose?: boolean;
}

const ImagePreviewModal = ({
  imageId,
  thumbnailUri,
  visible,
  onClose,
  enableSwipeToClose = true,
}: IImagePreviewModal) => {
  const { data: fullImageUri } = useGetSiteImageFull({
    imageId: visible ? imageId : undefined,
  });
  const imageUri = fullImageUri ?? thumbnailUri;

  return (
    <Modal
      visible={visible}
      animationType="slide"
      presentationStyle="pageSheet"
      allowSwipeDismissal={enableSwipeToClose}
      onRequestClose={onClose}
    >
      <GestureHandlerRootView style={imagePreviewModalStyles.gestureRoot}>
        <SafeAreaView style={imagePreviewModalStyles.container}>
          <StatusBar hidden />
          <ImagePreviewHeader onClose={onClose} />
          <ZoomablePreviewImage imageId={imageId} imageUri={imageUri} />
        </SafeAreaView>
      </GestureHandlerRootView>
    </Modal>
  );
};

export default ImagePreviewModal;
