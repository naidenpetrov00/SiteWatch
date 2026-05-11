import {
  Image as ReactNativeImage,
  type LayoutChangeEvent,
} from "react-native";
import {
  useAnimatedStyle,
  useSharedValue,
  withTiming,
} from "react-native-reanimated";
import { useCallback, useEffect, useMemo } from "react";

import { Gesture } from "react-native-gesture-handler";
import {
  DOUBLE_TAP_SCALE,
  MAX_SCALE,
  MIN_SCALE,
  clamp,
  getMaxTranslate,
} from "../zoomUtils";

const measureRenderedImage = (
  imageUri: string,
  viewportWidth: number,
  viewportHeight: number,
  onMeasured: (width: number, height: number) => void,
) => {
  if (!viewportWidth || !viewportHeight) {
    return;
  }

  ReactNativeImage.getSize(
    imageUri,
    (imageWidth, imageHeight) => {
      const viewportAspectRatio = viewportWidth / viewportHeight;
      const imageAspectRatio = imageWidth / imageHeight;

      if (imageAspectRatio > viewportAspectRatio) {
        onMeasured(viewportWidth, viewportWidth / imageAspectRatio);
        return;
      }

      onMeasured(viewportHeight * imageAspectRatio, viewportHeight);
    },
    () => {},
  );
};

export const useZoomableImage = ({
  imageId,
  imageUri,
}: {
  imageId: string;
  imageUri: string;
}) => {
  const scale = useSharedValue(1);
  const savedScale = useSharedValue(1);
  const translateX = useSharedValue(0);
  const translateY = useSharedValue(0);
  const savedTranslateX = useSharedValue(0);
  const savedTranslateY = useSharedValue(0);
  const viewportWidth = useSharedValue(0);
  const viewportHeight = useSharedValue(0);
  const renderedImageWidth = useSharedValue(0);
  const renderedImageHeight = useSharedValue(0);

  useEffect(() => {
    scale.value = 1;
    savedScale.value = 1;
    translateX.value = 0;
    translateY.value = 0;
    savedTranslateX.value = 0;
    savedTranslateY.value = 0;
    renderedImageWidth.value = 0;
    renderedImageHeight.value = 0;
  }, [
    imageId,
    renderedImageHeight,
    renderedImageWidth,
    savedScale,
    savedTranslateX,
    savedTranslateY,
    scale,
    translateX,
    translateY,
  ]);

  useEffect(() => {
    measureRenderedImage(
      imageUri,
      viewportWidth.value,
      viewportHeight.value,
      (width, height) => {
        renderedImageWidth.value = width;
        renderedImageHeight.value = height;
      },
    );
  }, [
    imageUri,
    renderedImageHeight,
    renderedImageWidth,
    viewportHeight,
    viewportWidth,
  ]);

  const handleImageWrapperLayout = useCallback(
    ({ nativeEvent }: LayoutChangeEvent) => {
      const { width, height } = nativeEvent.layout;

      viewportWidth.value = width;
      viewportHeight.value = height;

      measureRenderedImage(imageUri, width, height, (imageWidth, imageHeight) => {
        renderedImageWidth.value = imageWidth;
        renderedImageHeight.value = imageHeight;
      });
    },
    [
      imageUri,
      renderedImageHeight,
      renderedImageWidth,
      viewportHeight,
      viewportWidth,
    ],
  );

  const pinchGesture = useMemo(
    () =>
      Gesture.Pinch()
        .onUpdate((event) => {
          scale.value = clamp(
            savedScale.value * event.scale,
            MIN_SCALE,
            MAX_SCALE,
          );

          if (scale.value === MIN_SCALE) {
            translateX.value = 0;
            translateY.value = 0;
            return;
          }

          const maxTranslateX = getMaxTranslate(
            renderedImageWidth.value,
            viewportWidth.value,
            scale.value,
          );
          const maxTranslateY = getMaxTranslate(
            renderedImageHeight.value,
            viewportHeight.value,
            scale.value,
          );

          translateX.value = clamp(
            translateX.value,
            -maxTranslateX,
            maxTranslateX,
          );
          translateY.value = clamp(
            translateY.value,
            -maxTranslateY,
            maxTranslateY,
          );
        })
        .onEnd(() => {
          savedScale.value = scale.value;

          if (scale.value === MIN_SCALE) {
            translateX.value = withTiming(0);
            translateY.value = withTiming(0);
            savedTranslateX.value = 0;
            savedTranslateY.value = 0;
            return;
          }

          const maxTranslateX = getMaxTranslate(
            renderedImageWidth.value,
            viewportWidth.value,
            scale.value,
          );
          const maxTranslateY = getMaxTranslate(
            renderedImageHeight.value,
            viewportHeight.value,
            scale.value,
          );

          translateX.value = withTiming(
            clamp(translateX.value, -maxTranslateX, maxTranslateX),
          );
          translateY.value = withTiming(
            clamp(translateY.value, -maxTranslateY, maxTranslateY),
          );
          savedTranslateX.value = clamp(
            translateX.value,
            -maxTranslateX,
            maxTranslateX,
          );
          savedTranslateY.value = clamp(
            translateY.value,
            -maxTranslateY,
            maxTranslateY,
          );
        }),
    [
      renderedImageHeight,
      renderedImageWidth,
      savedScale,
      savedTranslateX,
      savedTranslateY,
      scale,
      translateX,
      translateY,
      viewportHeight,
      viewportWidth,
    ],
  );

  const panGesture = useMemo(
    () =>
      Gesture.Pan()
        .onUpdate((event) => {
          if (scale.value <= MIN_SCALE) {
            return;
          }

          const maxTranslateX = getMaxTranslate(
            renderedImageWidth.value,
            viewportWidth.value,
            scale.value,
          );
          const maxTranslateY = getMaxTranslate(
            renderedImageHeight.value,
            viewportHeight.value,
            scale.value,
          );

          translateX.value = clamp(
            savedTranslateX.value + event.translationX,
            -maxTranslateX,
            maxTranslateX,
          );
          translateY.value = clamp(
            savedTranslateY.value + event.translationY,
            -maxTranslateY,
            maxTranslateY,
          );
        })
        .onEnd(() => {
          if (scale.value <= MIN_SCALE) {
            translateX.value = withTiming(0);
            translateY.value = withTiming(0);
            savedTranslateX.value = 0;
            savedTranslateY.value = 0;
            return;
          }

          savedTranslateX.value = translateX.value;
          savedTranslateY.value = translateY.value;
        }),
    [
      renderedImageHeight,
      renderedImageWidth,
      savedTranslateX,
      savedTranslateY,
      scale,
      translateX,
      translateY,
      viewportHeight,
      viewportWidth,
    ],
  );

  const doubleTapGesture = useMemo(
    () =>
      Gesture.Tap()
        .numberOfTaps(2)
        .onEnd((event) => {
          if (scale.value > MIN_SCALE) {
            scale.value = withTiming(1);
            savedScale.value = 1;
            translateX.value = withTiming(0);
            translateY.value = withTiming(0);
            savedTranslateX.value = 0;
            savedTranslateY.value = 0;
            return;
          }

          const targetScale = DOUBLE_TAP_SCALE;
          const maxTranslateX = getMaxTranslate(
            renderedImageWidth.value,
            viewportWidth.value,
            targetScale,
          );
          const maxTranslateY = getMaxTranslate(
            renderedImageHeight.value,
            viewportHeight.value,
            targetScale,
          );
          const targetTranslateX = clamp(
            (viewportWidth.value / 2 - event.x) * (targetScale - 1),
            -maxTranslateX,
            maxTranslateX,
          );
          const targetTranslateY = clamp(
            (viewportHeight.value / 2 - event.y) * (targetScale - 1),
            -maxTranslateY,
            maxTranslateY,
          );

          scale.value = withTiming(targetScale);
          savedScale.value = targetScale;
          translateX.value = withTiming(targetTranslateX);
          translateY.value = withTiming(targetTranslateY);
          savedTranslateX.value = targetTranslateX;
          savedTranslateY.value = targetTranslateY;
        }),
    [
      renderedImageHeight,
      renderedImageWidth,
      savedScale,
      savedTranslateX,
      savedTranslateY,
      scale,
      translateX,
      translateY,
      viewportHeight,
      viewportWidth,
    ],
  );

  const imageGesture = useMemo(
    () => Gesture.Simultaneous(pinchGesture, panGesture, doubleTapGesture),
    [doubleTapGesture, panGesture, pinchGesture],
  );

  const animatedImageStyle = useAnimatedStyle(() => ({
    transform: [
      { translateX: translateX.value },
      { translateY: translateY.value },
      { scale: scale.value },
    ],
  }));

  return {
    animatedImageStyle,
    handleImageWrapperLayout,
    imageGesture,
  };
};
