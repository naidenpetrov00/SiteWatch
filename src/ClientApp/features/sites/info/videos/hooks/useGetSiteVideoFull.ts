import { useEffect, useState } from "react";

import type { SiteVideoPreviewCache } from "./useSiteVideoPreviewCache";

type UseGetSiteVideoFullOptions = {
  cache: SiteVideoPreviewCache;
  videoId?: string;
  visible?: boolean;
};

type VideoPreviewState = {
  error: string | null;
  isFetching: boolean;
  uri: string | null;
};

export const useGetSiteVideoFull = ({
  cache,
  videoId,
  visible = true,
}: UseGetSiteVideoFullOptions) => {
  const [state, setState] = useState<VideoPreviewState>({
    error: null,
    isFetching: false,
    uri: null,
  });

  useEffect(() => {
    let cancelled = false;

    if (!visible || !videoId) {
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
        const uri = await cache.getPreview(videoId);

        if (cancelled) {
          return;
        }

        setState({
          error: null,
          isFetching: false,
          uri,
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
  }, [cache, videoId, visible]);

  return state;
};

export default useGetSiteVideoFull;
