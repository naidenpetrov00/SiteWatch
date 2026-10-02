import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { firstValueFrom, debounceTime, distinctUntilChanged } from 'rxjs';

import { DashboardProductLookup } from '../../../products/models/dashboard-product.models';
import { DashboardProductsService } from '../../../products/services/dashboard-products.service';
import { DashboardRetailerLookup } from '../../../retailers/models/dashboard-retailer.models';
import { DashboardRetailersService } from '../../../retailers/services/dashboard-retailers.service';
import {
  RetailerListing,
  RetailerListingDialogData,
  RetailerListingFixedProduct,
  RetailerListingFixedRetailer,
  RetailerPriceBasis
} from '../../models/retailer-listing.models';
import {
  RetailerListingsService,
  getRetailerListingError
} from '../../services/retailer-listings.service';
import { RetailerPriceHistoryComponent } from '../retailer-price-history/retailer-price-history.component';
import {
  RetailerListingStatusConfirmDialogComponent,
  RetailerListingStatusConfirmDialogData
} from './retailer-listing-status-confirm-dialog.component';

@Component({
  selector: 'app-retailer-listing-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    RetailerPriceHistoryComponent
  ],
  templateUrl: './retailer-listing-dialog.component.html',
  styleUrl: './retailer-listing-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerListingDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);
  private readonly dialogRef = inject(
    MatDialogRef<RetailerListingDialogComponent, RetailerListing | null>
  );
  private readonly productsService = inject(DashboardProductsService);
  private readonly retailersService = inject(DashboardRetailersService);
  private readonly listingsService = inject(RetailerListingsService);
  readonly data = inject<RetailerListingDialogData>(MAT_DIALOG_DATA);
  private searchRevision = 0;

  readonly listing = signal(this.data.listing);
  readonly selectedProduct = signal<RetailerListingFixedProduct | null>(
    this.data.fixedProduct ?? this.productFromListing(this.data.listing)
  );
  readonly selectedRetailer = signal<RetailerListingFixedRetailer | null>(
    this.data.fixedRetailer ?? this.retailerFromListing(this.data.listing)
  );
  readonly productResults = signal<readonly DashboardProductLookup[]>([]);
  readonly retailerResults = signal<readonly DashboardRetailerLookup[]>([]);
  readonly saveError = signal<string | null>(null);
  readonly saveSuccess = signal<string | null>(null);
  readonly historyRefreshRevision = signal(0);
  readonly statusPending = signal(false);
  readonly saving = signal(false);
  readonly historyExpanded = signal(false);

  readonly counterpartControl = this.formBuilder.control<
    string | DashboardProductLookup | DashboardRetailerLookup | null
  >(null, Validators.required);
  readonly form = this.formBuilder.group(
    {
      productUrl: this.formBuilder.nonNullable.control(
        this.data.listing?.productUrl ?? '',
        [Validators.maxLength(2048), optionalHttpUrlValidator()]
      ),
      retailerProductCode: this.formBuilder.nonNullable.control(
        this.data.listing?.retailerProductCode ?? '',
        [Validators.maxLength(100)]
      ),
      amount: this.formBuilder.control<number | null>(null, [
        Validators.min(0.01),
        maximumTwoDecimalsValidator()
      ]),
      basis: this.formBuilder.control<RetailerPriceBasis | null>(null)
    },
    { validators: [optionalPricePairValidator()] }
  );

  readonly isEditing = computed(() => this.listing() !== null);
  readonly title = computed(() =>
    this.isEditing() ? 'Manage retailer listing' : 'Add retailer listing'
  );
  readonly priceEntryEnabled = computed(() => {
    const listing = this.listing();
    return listing === null || (
      listing.isActive &&
      listing.retailerIsActive &&
      listing.productStatus === 'Active'
    );
  });
  readonly allowedBases = computed<readonly RetailerPriceBasis[]>(() => {
    const product = this.selectedProduct();
    const packageQuantity =
      product?.packageQuantity ?? this.listing()?.productPackageQuantity ?? null;
    const packageUnit =
      product?.packageUnit ?? this.listing()?.productPackageUnit ?? null;
    if (packageQuantity === null) return ['item'];
    return packageUnit === 'piece' ? ['item', 'package'] : ['package'];
  });
  readonly displayCounterpart = (
    value: string | DashboardProductLookup | DashboardRetailerLookup | null
  ): string => {
    if (typeof value === 'string' || value === null) return value ?? '';
    return 'title' in value
      ? '#' + value.numberId + ' · ' + value.title
      : value.displayName;
  };

  constructor() {
    if (!this.data.listing) {
      this.counterpartControl.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe((value) => {
          if (typeof value !== 'string') return;
          this.searchRevision += 1;
          if (this.data.fixedProduct) this.selectedRetailer.set(null);
          if (this.data.fixedRetailer) this.selectedProduct.set(null);
        });

      this.counterpartControl.valueChanges
        .pipe(
          debounceTime(250),
          distinctUntilChanged(),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe((value) => {
          if (typeof value === 'string') {
            void this.searchCounterparts(value, this.searchRevision);
          }
        });
    } else {
      this.counterpartControl.clearValidators();
    }

    effect(() => {
      const active = this.priceEntryEnabled();
      const controls = [this.form.controls.amount, this.form.controls.basis];
      for (const control of controls) {
        active
          ? control.enable({ emitEvent: false })
          : control.disable({ emitEvent: false });
      }
    });
  }

  onCounterpartSelected(event: MatAutocompleteSelectedEvent): void {
    const value = event.option.value as DashboardProductLookup | DashboardRetailerLookup;
    this.searchRevision += 1;
    this.productResults.set([]);
    this.retailerResults.set([]);
    this.counterpartControl.setValue(value, { emitEvent: false });
    if ('title' in value) {
      this.selectedProduct.set(value);
    } else {
      this.selectedRetailer.set(value);
    }
    const bases = this.allowedBases();
    if (
      this.form.controls.amount.value !== null &&
      !bases.includes(this.form.controls.basis.value!)
    ) {
      this.form.controls.basis.setValue(bases[0]);
    }
  }

  productLabel(): string {
    const product = this.selectedProduct();
    return product
      ? '#' + product.numberId + ' · ' + product.title
      : this.listing()
        ? '#' + this.listing()!.productNumberId + ' · ' + this.listing()!.productTitle
        : '';
  }

  retailerLabel(): string {
    return (
      this.selectedRetailer()?.displayName ??
      this.listing()?.retailerDisplayName ??
      ''
    );
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'EUR'
    }).format(amount);
  }

  formatDateTime(value: string): string {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  close(): void {
    this.dialogRef.close(null);
  }

  canSave(): boolean {
    return (
      !this.saving() &&
      !this.statusPending() &&
      this.form.valid &&
      this.selectedProduct() !== null &&
      this.selectedRetailer() !== null
    );
  }

  async save(): Promise<void> {
    if (!this.canSave()) {
      this.form.markAllAsTouched();
      this.counterpartControl.markAsTouched();
      return;
    }

    this.saveError.set(null);
    this.saveSuccess.set(null);
    this.saving.set(true);
    const value = this.form.getRawValue();
    const current = this.listing();
    const amount = this.priceEntryEnabled() ? value.amount : null;
    const basis = amount === null ? null : value.basis;
    let saved: RetailerListing;
    try {
      saved = current
        ? await this.listingsService.update({
            listingId: current.id,
            productUrl: normalizeOptional(value.productUrl),
            retailerProductCode: normalizeOptional(value.retailerProductCode),
            amount,
            basis
          })
        : await this.listingsService.create({
            productId: this.selectedProduct()!.id,
            retailerId: this.selectedRetailer()!.id,
            productUrl: normalizeOptional(value.productUrl),
            retailerProductCode: normalizeOptional(value.retailerProductCode),
            amount,
            basis
          });
    } catch (error) {
      this.saveError.set(getRetailerListingError(error));
      return;
    } finally {
      this.saving.set(false);
    }

    if (current && amount !== null) {
      this.listing.set(saved);
      this.form.controls.amount.reset(null);
      this.form.controls.basis.reset(null);
      this.form.markAsPristine();
      this.historyRefreshRevision.update((revision) => revision + 1);
      this.historyExpanded.set(true);
      this.saveSuccess.set('Manual price saved. Inspect price history below for the new observation.');
    } else {
      this.dialogRef.close(saved);
    }
  }

  async changeStatus(): Promise<void> {
    const current = this.listing();
    if (!current || this.statusPending()) return;
    const activate = !current.isActive;
    const confirmRef = this.dialog.open<
      RetailerListingStatusConfirmDialogComponent,
      RetailerListingStatusConfirmDialogData,
      boolean
    >(RetailerListingStatusConfirmDialogComponent, {
      autoFocus: false,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        label: this.productLabel() + ' at ' + this.retailerLabel(),
        activate
      }
    });
    if (!(await firstValueFrom(confirmRef.afterClosed()))) return;

    this.statusPending.set(true);
    this.saveError.set(null);
    try {
      this.listing.set(await this.listingsService.setActive(current, activate));
    } catch (error) {
      this.saveError.set(
        getRetailerListingError(error, 'The listing status could not be changed.')
      );
    } finally {
      this.statusPending.set(false);
    }
  }

  private async searchCounterparts(
    searchTerm: string,
    revision: number
  ): Promise<void> {
    const normalized = searchTerm.trim();
    if (!normalized) {
      this.productResults.set([]);
      this.retailerResults.set([]);
      return;
    }
    try {
      if (this.data.fixedProduct) {
        const results = await this.retailersService.searchRetailers(normalized);
        if (revision === this.searchRevision) this.retailerResults.set(results);
      } else {
        const results = await this.productsService.searchProducts(normalized);
        if (revision === this.searchRevision) this.productResults.set(results);
      }
    } catch {
      if (revision === this.searchRevision) {
        this.productResults.set([]);
        this.retailerResults.set([]);
      }
    }
  }

  private productFromListing(
    listing: RetailerListing | null
  ): RetailerListingFixedProduct | null {
    if (!listing) return null;
    return {
      id: listing.productId,
      numberId: listing.productNumberId,
      title: listing.productTitle,
      category: 'other',
      brand: listing.productBrand,
      model: listing.productModel,
      packageQuantity: listing.productPackageQuantity,
      packageUnit: listing.productPackageUnit as DashboardProductLookup['packageUnit']
    };
  }

  private retailerFromListing(
    listing: RetailerListing | null
  ): RetailerListingFixedRetailer | null {
    if (!listing) return null;
    return {
      id: listing.retailerId,
      displayName: listing.retailerDisplayName,
      baseWebsiteUrl: listing.retailerBaseWebsiteUrl,
      websiteHost: new URL(listing.retailerBaseWebsiteUrl).host
    };
  }
}

function normalizeOptional(value: string): string | null {
  const normalized = value.trim();
  return normalized || null;
}

function optionalPricePairValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const amount = control.get('amount')?.value;
    const basis = control.get('basis')?.value;
    return (amount === null && basis === null) || (amount !== null && basis)
      ? null
      : { pricePair: true };
  };
}

function maximumTwoDecimalsValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return value === null ||
      Math.abs(Math.round(value * 100) - value * 100) < Number.EPSILON * 100
      ? null
      : { decimalPlaces: true };
  };
}

function optionalHttpUrlValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '').trim();
    if (!value) return null;
    try {
      const url = new URL(value);
      return (url.protocol === 'http:' || url.protocol === 'https:') &&
        !url.username &&
        !url.password &&
        !url.hash
        ? null
        : { url: true };
    } catch {
      return { url: true };
    }
  };
}
