import { useEffect, useRef, useState } from "react";

const useOverlayVisibility = (isLandscape: boolean) => {
  const [overlayVisible, setOverlayVisible] = useState(false);
  const overlayHideTimeout = useRef<NodeJS.Timeout | null>(null);

  const clearOverlayTimeout = () => {
    if (overlayHideTimeout.current) {
      clearTimeout(overlayHideTimeout.current);
      overlayHideTimeout.current = null;
    }
  };

  const resetOverlayTimeout = () => {
    clearOverlayTimeout();
    overlayHideTimeout.current = setTimeout(() => {
      setOverlayVisible(false);
    }, 3000);
  };

  const handleOverlayPress = () => {
    setOverlayVisible((prev) => {
      const next = !prev;
      if (next) {
        resetOverlayTimeout();
      } else {
        clearOverlayTimeout();
      }
      return next;
    });
  };

  useEffect(() => {
    if (!overlayVisible) {
      resetOverlayTimeout();
    } else {
      clearOverlayTimeout();
    }

    return () => {
      clearOverlayTimeout();
    };
  }, [overlayVisible]);

  useEffect(() => {
    if (isLandscape) {
      setOverlayVisible(true);
      resetOverlayTimeout();
    }
  }, [isLandscape]);

  useEffect(() => {
    return () => clearOverlayTimeout();
  }, []);

  return {
    overlayVisible,
    handleOverlayPress,
    onInteraction: resetOverlayTimeout,
  };
};

export default useOverlayVisibility;
