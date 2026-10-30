import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

import { RetailerPriceCollectionRunSummary } from '../../../retailers/models/retailer-price-collection.models';

@Component({
  selector: 'app-offer-pricing-matrix-collection-runs',
  imports: [MatButtonModule, RouterLink],
  templateUrl: './offer-pricing-matrix-collection-runs.component.html',
  styleUrl: './offer-pricing-matrix-collection-runs.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPricingMatrixCollectionRunsComponent {
  readonly runs = input<readonly RetailerPriceCollectionRunSummary[]>([]);
  readonly hasLoadError = input(false);

  runProgress(processedCount: number, totalCount: number): number {
    return totalCount === 0 ? 100 : Math.round((processedCount / totalCount) * 100);
  }

  statusLabel(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').replace(/^./, value => value.toUpperCase());
  }
}
