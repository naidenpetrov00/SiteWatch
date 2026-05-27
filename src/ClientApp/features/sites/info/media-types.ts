export const ALL_FILTER = "All" as const;

export const MEDIA_CATEGORIES = [
  "Pipes",
  "Electricity",
  "Design",
  "Other",
] as const;

export type MediaCategory = (typeof MEDIA_CATEGORIES)[number];
export type MediaFilter = typeof ALL_FILTER | MediaCategory;
