import Animated from "react-native-reanimated";
import { GestureDetector } from "react-native-gesture-handler";
import { View } from "react-native";
import { imagePreviewModalStyles } from "./ImagePreviewModal.styles";
import { useZoomableImage } from "./hooks/useZoomableImage";

interface IZoomablePreviewImage {
  imageId: string;
  imageUri: string;
}

const ZoomablePreviewImage = ({
  imageId,
  imageUri,
}: IZoomablePreviewImage) => {
  const { animatedImageStyle, handleImageWrapperLayout, imageGesture } =
    useZoomableImage({ imageId, imageUri });

  return (
    <View
      onLayout={handleImageWrapperLayout}
      style={imagePreviewModalStyles.imageWrapper}
    >
      <GestureDetector gesture={imageGesture}>
        <Animated.Image
          source={{ uri: imageUri }}
          resizeMode="contain"
          style={[imagePreviewModalStyles.image, animatedImageStyle]}
        />
      </GestureDetector>
    </View>
  );
};

export default ZoomablePreviewImage;
