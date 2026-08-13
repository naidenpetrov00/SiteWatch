export const SITE_MEDIA_POLICY_PRESETS = [
  'ApartmentRenovation',
  'HouseBuild',
  'CommercialBuild',
  'SiteMaintenance',
  'Custom'
] as const;

export const OTHER_MEDIA_CATEGORY = 'Other';
export const ALL_MEDIA_FILTER = 'All';
export const MAX_MEDIA_CATEGORY_LENGTH = 50;
export const MAX_MEDIA_CATEGORY_COUNT = 20;

export type SiteMediaPolicyPreset = (typeof SITE_MEDIA_POLICY_PRESETS)[number];

export interface SiteMediaPolicyPresetDefinition {
  preset: SiteMediaPolicyPreset;
  displayName: string;
  categories: readonly string[];
}

export interface SiteMediaPolicy {
  preset: SiteMediaPolicyPreset;
  categories: readonly string[];
}

export function normalizeMediaCategory(value: string): string {
  return value.trim().replace(/\s+/g, ' ');
}

export function formatMediaPolicyPreset(preset: SiteMediaPolicyPreset): string {
  const labels: Record<SiteMediaPolicyPreset, string> = {
    ApartmentRenovation: 'Apartment Renovation',
    HouseBuild: 'House Build',
    CommercialBuild: 'Commercial Build',
    SiteMaintenance: 'Site Maintenance',
    Custom: 'Custom'
  };

  return labels[preset];
}
