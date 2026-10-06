import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
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
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  DashboardProductLookup,
  PRODUCT_PACKAGE_UNIT_OPTIONS
} from '../../../products/models/dashboard-product.models';
import {
  ProductDialogComponent,
  ProductDialogResult
} from '../../../products/components/product-dialog/product-dialog.component';
import { DashboardProductsService } from '../../../products/services/dashboard-products.service';
import {
  ActivityProductRequirement,
  ProductQuantityBehavior
} from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { getActivityCatalogError } from '../../utils/activity-catalog-error';

export interface ProductRequirementDialogData {
  activityId: string;
  sectionId: string;
  excludedProductIds: readonly string[];
  requirement?: ActivityProductRequirement;
}

@Component({
  selector: 'app-product-requirement-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './product-requirement-dialog.component.html',
  styleUrl: './product-requirement-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductRequirementDialogComponent {
  readonly data = inject<ProductRequirementDialogData>(MAT_DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(
    MatDialogRef<ProductRequirementDialogComponent>
  );
  private readonly dialog = inject(MatDialog);
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogService = inject(ActivityCatalogService);
  private readonly productsService = inject(DashboardProductsService);

  private searchRevision = 0;

  readonly searchResults = signal<readonly DashboardProductLookup[]>([]);
  readonly selectedProduct = signal<DashboardProductLookup | null>(null);
  readonly selectedProductStatus = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly productSearchControl = this.formBuilder.control<
    string | DashboardProductLookup | null
  >('');
  readonly formId = this.data.requirement
    ? 'edit-product-requirement-form'
    : 'add-product-requirement-form';
  readonly title = this.data.requirement ? 'Edit Product Requirement' : 'Add Product';
  readonly subtitle = this.data.requirement
    ? 'Update how this product is used by the measurement section.'
    : 'Search the active Product Catalog and configure the required quantity.';
  readonly submitLabel = this.data.requirement ? 'Save Requirement' : 'Add Product';
  readonly form = this.formBuilder.group({
    productId: this.formBuilder.nonNullable.control(
      this.data.requirement?.productId ?? '',
      [Validators.required]
    ),
    quantity: this.formBuilder.nonNullable.control(
      this.data.requirement?.quantity ?? 1,
      [Validators.required, Validators.min(Number.MIN_VALUE), fourDecimalPlaces()]
    ),
    isRequired: this.formBuilder.nonNullable.control(
      this.data.requirement?.isRequired ?? true
    ),
    quantityBehavior: this.formBuilder.nonNullable.control<ProductQuantityBehavior>(
      this.data.requirement?.quantityBehavior ?? 'proportional',
      [Validators.required]
    ),
    notes: this.formBuilder.nonNullable.control(
      this.data.requirement?.notes ?? '',
      [Validators.maxLength(1000)]
    )
  });

  readonly isSaving = (): boolean => this.catalogService.mutation.isPending();
  readonly displayProduct = (
    value: string | DashboardProductLookup | null
  ): string => (typeof value === 'string' ? value : value?.title ?? '');

  constructor() {
    if (!this.data.requirement) {
      this.productSearchControl.valueChanges
        .pipe(
          debounceTime(250),
          distinctUntilChanged(),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe((value) => {
          if (typeof value !== 'string') return;
          this.form.controls.productId.setValue('', { emitEvent: false });
          this.selectedProduct.set(null);
          this.selectedProductStatus.set(null);
          void this.searchProducts(value);
        });
    }
  }

  onProductSelected(event: MatAutocompleteSelectedEvent): void {
    const product = event.option.value as DashboardProductLookup;
    this.searchRevision += 1;
    this.searchResults.set([]);
    this.selectedProduct.set(product);
    this.selectedProductStatus.set(null);
    this.form.controls.productId.setValue(product.id, { emitEvent: false });
  }

  async createProduct(): Promise<void> {
    if (this.data.requirement) return;

    this.errorMessage.set(null);
    const dialogRef = this.dialog.open<
      ProductDialogComponent,
      null,
      ProductDialogResult
    >(ProductDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '52rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: null
    });
    const result = await firstValueFrom(dialogRef.afterClosed());
    if (!result || result === true) return;

    try {
      const product = await this.productsService.getProductById(result.createdProductId);
      const selected: DashboardProductLookup = {
        id: product.id,
        numberId: product.numberId,
        title: product.title,
        category: product.category,
        brand: product.brand,
        model: product.model,
        packageQuantity: product.packageQuantity,
        packageUnit: product.packageUnit
      };
      this.searchRevision += 1;
      this.searchResults.set([]);
      this.selectedProduct.set(selected);
      this.selectedProductStatus.set(product.status);
      this.productSearchControl.setValue(selected, { emitEvent: false });
      this.form.controls.productId.setValue(selected.id, { emitEvent: false });
      if (product.status !== 'Active') {
        this.errorMessage.set(
          'The product was created, but only active products can be attached to an activity.'
        );
      }
    } catch {
      this.errorMessage.set(
        'The product was created, but could not be loaded here. Search for it to attach it; do not create it again.'
      );
    }
  }

  close(): void {
    this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.productSearchControl.markAsTouched();
      return;
    }

    if (!this.data.requirement && this.selectedProductStatus() !== null &&
        this.selectedProductStatus() !== 'Active') {
      this.errorMessage.set('Only active products can be attached to an activity.');
      return;
    }

    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    const request = {
      quantity: value.quantity,
      isRequired: value.isRequired,
      quantityBehavior: value.quantityBehavior,
      notes: value.notes.trim() || null
    };

    try {
      if (this.data.requirement) {
        await this.catalogService.updateProductRequirement(
          this.data.activityId,
          this.data.sectionId,
          this.data.requirement.id,
          request
        );
      } else {
        await this.catalogService.createProductRequirement(
          this.data.activityId,
          this.data.sectionId,
          { productId: value.productId, ...request }
        );
      }
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(getActivityCatalogError(error));
    }
  }

  productIdentity(product: DashboardProductLookup): string {
    return [product.brand, product.model].filter(Boolean).join(' · ');
  }

  packageDescription(product: {
    packageQuantity: number | null;
    packageUnit: string | null;
  }): string {
    if (product.packageQuantity === null || product.packageUnit === null) {
      return 'Package not specified';
    }
    const unit = PRODUCT_PACKAGE_UNIT_OPTIONS.find(
      (option) => option.value === product.packageUnit
    );
    return `${product.packageQuantity} ${unit?.label ?? product.packageUnit}`;
  }

  private async searchProducts(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.searchRevision;
    this.searchResults.set([]);
    if (!searchTerm) return;

    try {
      const products = await this.productsService.searchProducts(searchTerm);
      if (revision !== this.searchRevision) return;
      const excludedIds = new Set(this.data.excludedProductIds);
      this.searchResults.set(
        products.filter((product) => !excludedIds.has(product.id))
      );
    } catch {
      if (revision === this.searchRevision) this.searchResults.set([]);
    }
  }
}

function fourDecimalPlaces(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = Number(control.value);
    if (!Number.isFinite(value)) return { decimalScale: true };
    const scaled = value * 10_000;
    return Math.abs(scaled - Math.round(scaled)) < 0.000001
      ? null
      : { decimalScale: true };
  };
}
