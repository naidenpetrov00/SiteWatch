export type RetailerExtractionProfileStatus = 'draft' | 'published';
export type RetailerExtractionRuleType = 'jsonLd' | 'cssSelector';
export type RetailerExtractionCssValueSource = 'textContent' | 'attribute';
export type RetailerExtractionDecimalSeparator = 'dot' | 'comma';
export type RetailerExtractionThousandsSeparator =
  | 'none'
  | 'dot'
  | 'comma'
  | 'space';
export type RetailerExtractionPriceBasis = 'item' | 'package';

export interface RetailerExtractionProfileSummary {
  id: string;
  companyPersonId: string;
  version: number;
  status: RetailerExtractionProfileStatus;
  isActive: boolean;
  created: string;
  lastModified: string;
  publishedAt: string | null;
  publishedBy: string | null;
  configurationRevision: number;
  validatedConfigurationRevision: number | null;
  isCurrentConfigurationValidated: boolean;
  lastSuccessfulTestAt: string | null;
  lastSuccessfulTestRuleId: string | null;
  ruleCount: number;
  enabledRuleCount: number;
}

export interface RetailerExtractionRule {
  id: string;
  name: string;
  isEnabled: boolean;
  priority: number;
  ruleType: RetailerExtractionRuleType;
  jsonLdObjectType: string | null;
  jsonLdPricePath: string | null;
  jsonLdCurrencyPath: string | null;
  cssSelector: string | null;
  cssValueSource: RetailerExtractionCssValueSource | null;
  cssAttributeName: string | null;
  decimalSeparator: RetailerExtractionDecimalSeparator;
  thousandsSeparator: RetailerExtractionThousandsSeparator;
  expectedCurrencyCode: 'EUR';
  priceBasis: RetailerExtractionPriceBasis;
  minimumValue: number | null;
  maximumValue: number | null;
}

export interface RetailerExtractionProfileDetails {
  summary: RetailerExtractionProfileSummary;
  allowedHosts: readonly string[];
  rules: readonly RetailerExtractionRule[];
}

export interface RetailerExtractionCurrentProfiles {
  owner: RetailerExtractionOwner;
  draft: RetailerExtractionProfileDetails | null;
  active: RetailerExtractionProfileDetails | null;
}

export interface RetailerExtractionRetailer {
  id: string;
  displayName: string;
  websiteHost: string;
  isActive: boolean;
}

export interface RetailerExtractionOwner {
  companyPersonId: string;
  companyDisplayName: string;
  retailerCount: number;
  retailers: readonly RetailerExtractionRetailer[];
}

export interface RetailerExtractionOverview {
  companyPersonId: string;
  companyDisplayName: string;
  retailerCount: number;
  activeProfileId: string | null;
  activeVersion: number | null;
  isCurrentConfigurationValidated: boolean | null;
}

export interface SaveRetailerExtractionRuleRequest {
  name: string;
  isEnabled: boolean;
  ruleType: RetailerExtractionRuleType;
  jsonLdObjectType: string | null;
  jsonLdPricePath: string | null;
  jsonLdCurrencyPath: string | null;
  cssSelector: string | null;
  cssValueSource: RetailerExtractionCssValueSource | null;
  cssAttributeName: string | null;
  decimalSeparator: RetailerExtractionDecimalSeparator;
  thousandsSeparator: RetailerExtractionThousandsSeparator;
  expectedCurrencyCode: 'EUR';
  priceBasis: RetailerExtractionPriceBasis;
  minimumValue: number | null;
  maximumValue: number | null;
}

export type RetailerExtractionTestSourceType = 'retailerListing' | 'manualUrl';

export interface TestRetailerExtractionProfileRequest {
  sourceType: RetailerExtractionTestSourceType;
  retailerListingId: string | null;
  manualUrl: string | null;
}

export interface RetailerExtractionRuleDiagnostic {
  ruleId: string;
  ruleName: string;
  ruleType: RetailerExtractionRuleType;
  priority: number;
  outcome: string;
  explanation: string;
}

export interface RetailerExtractionSuccess {
  amount: number;
  currencyCode: 'EUR';
  priceBasis: RetailerExtractionPriceBasis;
  profileId: string;
  ruleId: string;
  ruleName: string;
  rulePriority: number;
  rawValue: string;
}

export interface RetailerExtractionTestResult {
  testedAt: string;
  testedUrl: string;
  profileId: string;
  success: boolean;
  isCurrentConfigurationValidated: boolean;
  extraction: RetailerExtractionSuccess | null;
  diagnostics: readonly RetailerExtractionRuleDiagnostic[];
}
