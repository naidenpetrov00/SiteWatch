export const ALL_FILTER = "All" as const;

export type MediaCategory = string;
export type MediaFilter = typeof ALL_FILTER | MediaCategory;
