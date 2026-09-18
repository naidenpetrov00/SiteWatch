import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';

import { OfferProductLine } from '../../models/offer.models';
import {
  formatOfferQuantity,
  measurementUnitLabel,
  packageUnitLabel
} from '../../utils/offer-quantity';

@Component({
  selector: 'app-offer-product-requirements',
  imports: [MatExpansionModule, MatIconModule],
  templateUrl: './offer-product-requirements.component.html',
  styleUrl: './offer-product-requirements.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferProductRequirementsComponent {
  readonly products = input.required<readonly OfferProductLine[]>();

  formatQuantity = formatOfferQuantity;
  measurementUnit = measurementUnitLabel;

  productIdentity(product: OfferProductLine): string {
    return [product.brand, product.model].filter(Boolean).join(' · ');
  }

  packageDescription(product: OfferProductLine): string {
    if (product.packageQuantity === null || product.packageUnit === null) {
      return 'Package not specified';
    }
    return `${formatOfferQuantity(product.packageQuantity)} ${packageUnitLabel(product.packageUnit)}`;
  }
}
