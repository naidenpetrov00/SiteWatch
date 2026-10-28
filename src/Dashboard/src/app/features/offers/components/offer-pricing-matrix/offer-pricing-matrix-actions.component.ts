import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { DashboardRetailerLookup } from '../../../retailers/models/dashboard-retailer.models';
import { OfferPricingMatrix } from '../../models/offer.models';

@Component({
  selector: 'app-offer-pricing-matrix-actions',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './offer-pricing-matrix-actions.component.html',
  styleUrl: './offer-pricing-matrix-actions.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPricingMatrixActionsComponent {
  readonly matrix = input.required<OfferPricingMatrix>();
  readonly editable = input(false);
  readonly isMutating = input(false);
  readonly retailerSearch = input.required<FormControl<string | DashboardRetailerLookup | null>>();
  readonly retailerResults = input<readonly DashboardRetailerLookup[]>([]);
  readonly onlinePriceCollectionRequested = output<void>();
  readonly retailerSelected = output<DashboardRetailerLookup>();

  readonly displayRetailer = (value: string | DashboardRetailerLookup | null): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';

  selectRetailer(event: MatAutocompleteSelectedEvent): void {
    this.retailerSelected.emit(event.option.value as DashboardRetailerLookup);
  }
}
