import type { MediaCategory } from "../info/media-types";

export interface SiteMediaPolicy {
  preset:
    | "ApartmentRenovation"
    | "HouseBuild"
    | "CommercialBuild"
    | "SiteMaintenance"
    | "Custom";
  categories: MediaCategory[];
}

export interface Site {
  id: string;
  name: string;
  address: string;
  managerId: string;
  managerDisplayName: string;
  startDate: string;
  endDate: string | null;
  status: string;
  mediaPolicy: SiteMediaPolicy;
}
