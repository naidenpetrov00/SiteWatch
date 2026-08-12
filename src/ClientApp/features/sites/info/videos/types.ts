import type { MediaCategory } from "../media-types";

export type SiteVideoIds = {
  videoId: string;
  snapshotId: string;
  durationSeconds: number | null;
  category: MediaCategory;
  created: string;
};

export type VisibleSiteVideo = SiteVideoIds & {
  snapshotUri: string;
};
