import type { MediaCategory } from "../media-types";

export type SiteVideoIds = {
  videoId: string;
  snapshotId: string;
  durationSeconds: number | null;
  category: MediaCategory;
};

export type VisibleSiteVideo = SiteVideoIds & {
  snapshotUri: string;
};
