import { FILTERS } from "./Filters/Filters";

export type FilterType = (typeof FILTERS)[number];

export type GalleryItem = {
  id: string;
  title: string;
  category: Exclude<FilterType, "All">;
  color: string;
};
