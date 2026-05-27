import type { MediaCategory, MediaFilter } from "../../media-types";

export type FilterType = MediaFilter;

export type GalleryItem = {
  id: string;
  title: string;
  category: MediaCategory;
  color: string;
};
