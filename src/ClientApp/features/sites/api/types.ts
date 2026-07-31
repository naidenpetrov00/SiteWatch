import type { MediaCategory } from "../info/media-types";

export interface SiteMediaPolicy {
  preset: "Regular" | "Custom";
  allowedImageCategories: MediaCategory[];
  allowedVideoCategories: MediaCategory[];
}

export interface Site {
  id: string;
  name: string;
  address: string;
  mediaPolicy: SiteMediaPolicy;
}
