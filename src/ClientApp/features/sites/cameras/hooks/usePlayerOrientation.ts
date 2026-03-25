import * as ScreenOrientation from "expo-screen-orientation";

import { useEffect, useRef, useState } from "react";

const isOrientationLandscape = (orientation: ScreenOrientation.Orientation) => {
  return (
    orientation === ScreenOrientation.Orientation.LANDSCAPE_LEFT ||
    orientation === ScreenOrientation.Orientation.LANDSCAPE_RIGHT
  );
};

const usePlayerOrientation = () => {
  const [isLandscape, setIsLandscape] = useState(false);
  const initialOrientationLock = useRef<
    ScreenOrientation.OrientationLock | undefined
  >(undefined);

  useEffect(() => {
    const setInitialOrientation = async () => {
      const orientationLock = await ScreenOrientation.getOrientationLockAsync();
      initialOrientationLock.current = orientationLock;
      await ScreenOrientation.unlockAsync();
      const orientation = await ScreenOrientation.getOrientationAsync();
      setIsLandscape(isOrientationLandscape(orientation));
    };

    const subscription = ScreenOrientation.addOrientationChangeListener(
      (event) => {
        setIsLandscape(
          isOrientationLandscape(event.orientationInfo.orientation)
        );
      }
    );

    setInitialOrientation();

    return () => {
      ScreenOrientation.removeOrientationChangeListener(subscription);
      ScreenOrientation.lockAsync(
        initialOrientationLock.current ??
          ScreenOrientation.OrientationLock.PORTRAIT
      );
    };
  }, []);

  const toggleFullscreen = async () => {
    if (isLandscape) {
      await ScreenOrientation.lockAsync(
        initialOrientationLock.current ??
          ScreenOrientation.OrientationLock.PORTRAIT
      );
      setIsLandscape(false);
    } else {
      await ScreenOrientation.lockAsync(
        ScreenOrientation.OrientationLock.LANDSCAPE
      );
      setIsLandscape(true);
    }
  };

  return { isLandscape, toggleFullscreen };
};

export default usePlayerOrientation;
