import { FILTERS } from "@/app/(app)/Site/[siteId]/Images/SiteImages";

export type FilterType = (typeof FILTERS)[number];

export type GalleryItem = {
  id: string;
  title: string;
  category: Exclude<FilterType, "All">;
  color: string;
};