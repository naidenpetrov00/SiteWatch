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
  retailerId: string;
  version: number;
  status: RetailerExtractionProfileStatus;
  isActive: boolean;
  created: string;
  lastModified: string;
  publishedAt: string | null;
  publishedBy: string | null;
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
  draft: RetailerExtractionProfileDetails | null;
  active: RetailerExtractionProfileDetails | null;
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
