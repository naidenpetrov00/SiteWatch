import * as FileSystem from "expo-file-system/legacy";

import { useCallback, useEffect, useMemo, useRef } from "react";
import { Platform } from "react-native";

import { env } from "@/config/env";
import { paths } from "@/config/constants/paths";

const PREVIEW_CACHE_DIR = `${FileSystem.cacheDirectory ?? ""}site-video-previews/`;

const buildVideoUrl = (videoId: string) =>
  new URL(paths.videos.getById(videoId), env.API_URL).toString();

const getPreviewBaseName = (videoId: string) =>
  `site-video-${videoId}-${Date.now()}-${Math.random().toString(16).slice(2)}`;

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

  await FileSystem.makeDirectoryAsync(PREVIEW_CACHE_DIR, {
    intermediates: true,
  });

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

type CachedPreviewEntry = {
  cleanup?: () => Promise<void>;
  promise?: Promise<string>;
  uri?: string;
};

export type SiteVideoPreviewCache = {
  clear: () => Promise<void>;
  getPreview: (videoId: string) => Promise<string>;
};

type UseSiteVideoPreviewCacheOptions = {
  siteId?: string;
};

export const useSiteVideoPreviewCache = ({
  siteId,
}: UseSiteVideoPreviewCacheOptions): SiteVideoPreviewCache => {
  const cacheRef = useRef<Map<string, CachedPreviewEntry>>(new Map());
  const sessionRef = useRef(0);

  const clear = useCallback(async () => {
    sessionRef.current += 1;

    const entries = Array.from(cacheRef.current.values());
    cacheRef.current.clear();

    await Promise.allSettled(
      entries.map(async (entry) => {
        await entry.cleanup?.();
      }),
    );
  }, []);

  const getPreview = useCallback(
    async (videoId: string) => {
      const existing = cacheRef.current.get(videoId);

      if (existing?.uri) {
        return existing.uri;
      }

      if (existing?.promise) {
        return existing.promise;
      }

      const requestSession = sessionRef.current;

      const previewPromise = (async () => {
        const preview =
          Platform.OS === "web"
            ? await prepareWebPreview(videoId)
            : await prepareNativePreview(videoId);

        if (sessionRef.current !== requestSession) {
          await preview.cleanup();
          throw new Error("Video preview cache was cleared");
        }

        cacheRef.current.set(videoId, {
          cleanup: preview.cleanup,
          uri: preview.uri,
        });

        return preview.uri;
      })();

      cacheRef.current.set(videoId, {
        promise: previewPromise,
      });

      try {
        return await previewPromise;
      } catch (error) {
        const current = cacheRef.current.get(videoId);

        if (current?.promise === previewPromise) {
          cacheRef.current.delete(videoId);
        }

        throw error;
      }
    },
    [],
  );

  useEffect(() => {
    return () => {
      void clear();
    };
  }, [clear, siteId]);

  return useMemo(
    () => ({
      clear,
      getPreview,
    }),
    [clear, getPreview],
  );
};

export default useSiteVideoPreviewCache;
