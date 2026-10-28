import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { OfferPricingMatrix } from '../../models/offer.models';

@Component({
  selector: 'app-offer-pricing-matrix-summary',
  templateUrl: './offer-pricing-matrix-summary.component.html',
  styleUrl: './offer-pricing-matrix-summary.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPricingMatrixSummaryComponent {
  readonly matrix = input<OfferPricingMatrix>();

  formatCurrency(amount: number | null): string {
    return amount === null
      ? '—'
      : new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EUR' }).format(amount);
  }
}
