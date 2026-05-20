import * as FileSystem from "expo-file-system/legacy";

import { useEffect, useRef, useState } from "react";
import { Platform } from "react-native";

import { env } from "@/config/env";
import { paths } from "@/config/constants/paths";

const PREVIEW_CACHE_DIR = `${FileSystem.cacheDirectory ?? ""}site-video-previews/`;

const buildVideoUrl = (videoId: string) =>
  new URL(paths.videos.getById(videoId), env.API_URL).toString();

const getPreviewBaseName = (videoId: string) =>
  `site-video-${videoId}-${Date.now()}-${Math.random()
    .toString(16)
    .slice(2)}`;

const resolvePreviewExtension = (mimeType?: string | null) => {
  const normalizedMimeType = mimeType?.toLowerCase().split(";")[0]?.trim();

  switch (normalizedMimeType) {
    case "video/quicktime":
      return "mov";
    case "video/webm":
      return "webm";
    case "video/mp4":
    default:
      return "mp4";
  }
};

const deleteIfExists = async (uri: string) => {
  await FileSystem.deleteAsync(uri, { idempotent: true });
};

const prepareNativePreview = async (videoId: string) => {
  if (!FileSystem.cacheDirectory) {
    throw new Error("Cache directory is unavailable");
  }

  await FileSystem.makeDirectoryAsync(PREVIEW_CACHE_DIR, { intermediates: true });

  const baseName = getPreviewBaseName(videoId);
  const temporaryDownloadUri = `${PREVIEW_CACHE_DIR}${baseName}`;
  const downloadResult = await FileSystem.downloadAsync(
    buildVideoUrl(videoId),
    temporaryDownloadUri,
  );

  if (downloadResult.status < 200 || downloadResult.status >= 300) {
    await deleteIfExists(temporaryDownloadUri);
    throw new Error(`Video download failed with status ${downloadResult.status}`);
  }

  const extension = resolvePreviewExtension(
    downloadResult.mimeType ??
      downloadResult.headers["content-type"] ??
      downloadResult.headers["Content-Type"],
  );
  const finalUri = `${temporaryDownloadUri}.${extension}`;

  if (finalUri !== downloadResult.uri) {
    await FileSystem.moveAsync({
      from: downloadResult.uri,
      to: finalUri,
    });
  }

  return {
    cleanup: async () => {
      await deleteIfExists(finalUri);
    },
    uri: finalUri,
  };
};

const prepareWebPreview = async (videoId: string) => {
  const response = await fetch(buildVideoUrl(videoId));

  if (!response.ok) {
    throw new Error(`Video download failed with status ${response.status}`);
  }

  const blob = await response.blob();
  const objectUrl = URL.createObjectURL(blob);

  return {
    cleanup: async () => {
      URL.revokeObjectURL(objectUrl);
    },
    uri: objectUrl,
  };
};

type UseGetSiteVideoFullOptions = {
  videoId?: string;
  visible?: boolean;
};

type VideoPreviewState = {
  error: string | null;
  isFetching: boolean;
  uri: string | null;
};

export const useGetSiteVideoFull = ({
  videoId,
  visible = true,
}: UseGetSiteVideoFullOptions) => {
  const [state, setState] = useState<VideoPreviewState>({
    error: null,
    isFetching: false,
    uri: null,
  });
  const cleanupRef = useRef<(() => Promise<void>) | undefined>(undefined);

  useEffect(() => {
    let cancelled = false;
    const previousCleanup = cleanupRef.current;
    cleanupRef.current = undefined;

    if (!visible || !videoId) {
      void previousCleanup?.();
      setState({
        error: null,
        isFetching: false,
        uri: null,
      });
      return () => {
        cancelled = true;
      };
    }

    setState({
      error: null,
      isFetching: true,
      uri: null,
    });

    void (async () => {
      try {
        await previousCleanup?.();

        const preview =
          Platform.OS === "web"
            ? await prepareWebPreview(videoId)
            : await prepareNativePreview(videoId);

        if (cancelled) {
          await preview.cleanup();
          return;
        }

        cleanupRef.current = preview.cleanup;
        setState({
          error: null,
          isFetching: false,
          uri: preview.uri,
        });
      } catch (error) {
        if (cancelled) {
          return;
        }

        setState({
          error:
            error instanceof Error ? error.message : "Failed to load video",
          isFetching: false,
          uri: null,
        });
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [videoId, visible]);

  useEffect(
    () => () => {
      void cleanupRef.current?.();
      cleanupRef.current = undefined;
    },
    [],
  );

  return state;
};

export default useGetSiteVideoFull;
