import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { OfferOnlinePriceCollectionStartOutcome } from '../../models/offer.models';

@Component({
  selector: 'app-offer-pricing-matrix-status',
  imports: [MatProgressSpinnerModule],
  templateUrl: './offer-pricing-matrix-status.component.html',
  styleUrl: './offer-pricing-matrix-status.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPricingMatrixStatusComponent {
  readonly feedback = input<string | null>(null);
  readonly error = input<string | null>(null);
  readonly collectionOutcomes = input<readonly OfferOnlinePriceCollectionStartOutcome[]>([]);
  readonly loading = input(false);
  readonly loadError = input(false);
}
