import { FormControl, FormGroup } from '@angular/forms';

import {
  OfferPriceBasis,
  OfferPricingProductRow,
  OfferPricingRequirement,
  OfferRetailerPriceCell
} from '../../models/offer.models';

export type PriceCellForm = FormGroup<{
  amount: FormControl<number | null>;
  basis: FormControl<OfferPriceBasis>;
  productUrl: FormControl<string>;
  retailerProductCode: FormControl<string>;
}>;

export interface PricingActivityDisplayRow {
  kind: 'activity';
  id: string;
  activityNumberId: number;
  activityName: string;
}

export interface PricingSectionDisplayRow {
  kind: 'section';
  id: string;
  offerActivityId: string;
  sectionName: string | null;
}

export interface PricingProductDisplayRow {
  kind: 'product';
  id: string;
  product: OfferPricingProductRow;
  requirement: OfferPricingRequirement;
  sharedRequirementCount: number;
}

export type PricingDisplayRow =
  | PricingActivityDisplayRow
  | PricingSectionDisplayRow
  | PricingProductDisplayRow;

export interface PricingCellAction {
  row: OfferPricingProductRow;
  cell: OfferRetailerPriceCell;
}

export interface PricingEditAction extends PricingCellAction {
  displayRowId: string;
}
