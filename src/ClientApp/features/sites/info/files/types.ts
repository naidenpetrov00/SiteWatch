import type { MediaCategory } from "../media-types";

export type SiteFileIds = {
  fileId: string;
  fileName: string;
  contentType: string;
  category: MediaCategory;
};
