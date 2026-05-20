export type SiteVideoIds = {
  videoId: string;
  snapshotId: string;
  durationSeconds: number | null;
  category: string;
};

export type VisibleSiteVideo = SiteVideoIds & {
  snapshotUri: string;
};
