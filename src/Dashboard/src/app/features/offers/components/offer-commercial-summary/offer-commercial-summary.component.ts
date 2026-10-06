import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { OfferPricingMatrix } from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';

@Component({
  selector: 'app-offer-commercial-summary',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './offer-commercial-summary.component.html',
  styleUrl: './offer-commercial-summary.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferCommercialSummaryComponent {
  readonly offersService = inject(OffersService);
  private readonly formBuilder = inject(FormBuilder);
  private configuredOfferId: string | null = null;

  readonly siteId = input.required<string>();
  readonly offerId = input.required<string>();
  readonly matrix = input<OfferPricingMatrix>();
  readonly editable = input(false);
  readonly feedback = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly discountForm = this.formBuilder.nonNullable.group({
    activityDiscountPercentage: [0, [percentageValidator()]],
    productDiscountPercentage: [0, [percentageValidator()]]
  });

  constructor() {
    effect(() => {
      const offerId = this.offerId();
      const matrix = this.matrix();
      const editable = this.editable();
      if (!matrix) return;

      if (this.configuredOfferId !== offerId || !this.discountForm.dirty || !editable) {
        this.discountForm.reset(
          {
            activityDiscountPercentage:
              matrix.commercialTotals.activityDiscountPercentage,
            productDiscountPercentage:
              matrix.commercialTotals.productDiscountPercentage
          },
          { emitEvent: false }
        );
      }
      this.configuredOfferId = offerId;
      if (editable) {
        this.discountForm.enable({ emitEvent: false });
      } else {
        this.discountForm.disable({ emitEvent: false });
      }
    });
  }

  hasUnsavedEdits(): boolean {
    return this.discountForm.dirty;
  }

  isMutating(): boolean {
    return this.offersService.updateDiscountsMutation.isPending();
  }

  async save(): Promise<void> {
    if (!this.editable() || this.discountForm.invalid || this.isMutating()) {
      this.discountForm.markAllAsTouched();
      return;
    }

    this.feedback.set(null);
    this.error.set(null);
    const value = this.discountForm.getRawValue();
    try {
      await this.offersService.updateDiscounts({
        siteId: this.siteId(),
        offerId: this.offerId(),
        ...value
      });
      this.discountForm.markAsPristine();
      this.feedback.set('Offer discounts saved.');
    } catch (error) {
      this.error.set(getOfferError(error));
    }
  }

  formatCurrency(value: number | null): string {
    return value === null
      ? 'Unavailable'
      : new Intl.NumberFormat(undefined, {
          style: 'currency',
          currency: 'EUR'
        }).format(value);
  }
}

function percentageValidator(): ValidatorFn {
  const range = Validators.compose([
    Validators.required,
    Validators.min(0),
    Validators.max(100)
  ]);
  return (control: AbstractControl): ValidationErrors | null => {
    const rangeError = range?.(control);
    if (rangeError) return rangeError;
    const value = Number(control.value);
    return Math.abs(value * 100 - Math.round(value * 100)) < 0.000001
      ? null
      : { decimalPlaces: true };
  };
}
