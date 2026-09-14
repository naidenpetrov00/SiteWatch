import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal
} from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DialogWizardTabsComponent } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.component';
import { DialogWizardTabDefinition } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.types';
import {
  CreateDashboardProductRequest,
  DashboardProductDetails,
  PRODUCT_PACKAGE_UNIT_OPTIONS,
  PRODUCT_STATUSES,
  ProductPackageUnit,
  ProductStatus
} from '../../models/dashboard-product.models';
import { DashboardProductsService } from '../../services/dashboard-products.service';

type ProductDialogTabId = 'details' | 'search';
type SearchCollectionName =
  | 'alternativeSearchPhrases'
  | 'requiredKeywords'
  | 'excludedKeywords';

const PRODUCT_DIALOG_TABS = [
  { id: 'details', label: 'Product Details' },
  { id: 'search', label: 'Search Configuration' }
] as const satisfies readonly DialogWizardTabDefinition[];

@Component({
  selector: 'app-product-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DialogActionBarComponent,
    DialogShellComponent,
    DialogWizardTabsComponent
  ],
  templateUrl: './product-dialog.component.html',
  styleUrl: './product-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ProductDialogComponent>);
  private readonly productsService = inject(DashboardProductsService);
  private readonly product = inject<DashboardProductDetails | null>(MAT_DIALOG_DATA, {
    optional: true
  });

  readonly statuses = PRODUCT_STATUSES;
  readonly packageUnits = PRODUCT_PACKAGE_UNIT_OPTIONS;
  readonly tabs = PRODUCT_DIALOG_TABS;
  readonly selectedTabId = signal<ProductDialogTabId>('details');
  readonly formId = this.product
    ? 'edit-product-dialog-form'
    : 'add-product-dialog-form';
  readonly dialogTitle = this.product ? 'Edit Product' : 'Add Product';
  readonly dialogSubtitle = this.product
    ? `Update #${this.product.numberId} ${this.product.title}.`
    : 'Enter the product identity and future retailer-search configuration.';
  readonly submitLabel = this.product ? 'Save Product' : 'Add Product';
  readonly productForm = this.formBuilder.group(
    {
      title: this.formBuilder.nonNullable.control(this.product?.title ?? '', [
        Validators.required,
        Validators.maxLength(200)
      ]),
      description: this.formBuilder.nonNullable.control(
        this.product?.description ?? '',
        [Validators.maxLength(2000)]
      ),
      brand: this.formBuilder.nonNullable.control(this.product?.brand ?? '', [
        Validators.maxLength(100)
      ]),
      model: this.formBuilder.nonNullable.control(this.product?.model ?? '', [
        Validators.maxLength(100)
      ]),
      packageQuantity: this.formBuilder.control<number | null>(
        this.product?.packageQuantity ?? null,
        [Validators.min(Number.MIN_VALUE)]
      ),
      packageUnit: this.formBuilder.control<ProductPackageUnit | null>(
        this.product?.packageUnit ?? null
      ),
      category: this.formBuilder.nonNullable.control(this.product?.category ?? '', [
        Validators.required,
        Validators.maxLength(100)
      ]),
      status: this.formBuilder.nonNullable.control<ProductStatus>(
        this.product?.status ?? 'Active',
        [Validators.required]
      ),
      primarySearchPhrase: this.formBuilder.nonNullable.control(
        this.product?.primarySearchPhrase ?? '',
        [Validators.maxLength(250)]
      ),
      alternativeSearchPhrases: this.createSearchCollection(
        this.product?.alternativeSearchPhrases
      ),
      requiredKeywords: this.createSearchCollection(this.product?.requiredKeywords),
      excludedKeywords: this.createSearchCollection(this.product?.excludedKeywords)
    },
    {
      validators: [packagePairValidator()]
    }
  );

  readonly isSaving = (): boolean =>
    this.product
      ? this.productsService.updateProductMutation.isPending()
      : this.productsService.createProductMutation.isPending();

  setSelectedTab(tabId: string): void {
    if (this.tabs.some((tab) => tab.id === tabId)) {
      this.selectedTabId.set(tabId as ProductDialogTabId);
    }
  }

  addSearchEntry(collectionName: SearchCollectionName): void {
    const collection = this.productForm.controls[collectionName];
    if (collection.length >= 20) {
      return;
    }

    collection.push(this.createSearchEntry());
  }

  removeSearchEntry(collectionName: SearchCollectionName, index: number): void {
    this.productForm.controls[collectionName].removeAt(index);
  }

  effectivePrimarySearchPhrase(): string {
    const customPhrase = this.productForm.controls.primarySearchPhrase.value.trim();
    if (customPhrase) {
      return customPhrase;
    }

    return [
      this.productForm.controls.title.value,
      this.productForm.controls.brand.value,
      this.productForm.controls.model.value
    ]
      .map((value) => value.trim())
      .filter(Boolean)
      .join(' ');
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  async submitProduct(): Promise<void> {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const request = this.toRequest();
    try {
      if (this.product) {
        await this.productsService.updateProduct({ id: this.product.id, ...request });
      } else {
        await this.productsService.createProduct(request);
      }
      this.dialogRef.close(true);
    } catch {
      // Keep the form intact so the administrator can correct or retry it.
    }
  }

  private toRequest(): CreateDashboardProductRequest {
    const value = this.productForm.getRawValue();
    return {
      title: value.title.trim(),
      description: this.emptyToNull(value.description),
      brand: this.emptyToNull(value.brand),
      model: this.emptyToNull(value.model),
      packageQuantity: value.packageQuantity,
      packageUnit: value.packageUnit,
      category: value.category.trim(),
      status: value.status,
      primarySearchPhrase: this.emptyToNull(value.primarySearchPhrase),
      alternativeSearchPhrases: this.normalizeCollection(
        value.alternativeSearchPhrases
      ),
      requiredKeywords: this.normalizeCollection(value.requiredKeywords),
      excludedKeywords: this.normalizeCollection(value.excludedKeywords)
    };
  }

  private createSearchCollection(
    values: readonly string[] = []
  ): FormArray<FormControl<string>> {
    return this.formBuilder.array(values.map((value) => this.createSearchEntry(value)));
  }

  private createSearchEntry(value = ''): FormControl<string> {
    return this.formBuilder.nonNullable.control(value, [
      Validators.required,
      Validators.maxLength(200)
    ]);
  }

  private normalizeCollection(values: readonly string[]): string[] {
    const normalizedValues: string[] = [];
    for (const value of values) {
      const normalizedValue = value.trim().replace(/\s+/g, ' ');
      if (
        normalizedValue &&
        !normalizedValues.some(
          (existing) => existing.toLowerCase() === normalizedValue.toLowerCase()
        )
      ) {
        normalizedValues.push(normalizedValue);
      }
    }
    return normalizedValues;
  }

  private emptyToNull(value: string): string | null {
    return value.trim() || null;
  }
}

function packagePairValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const quantity = control.get('packageQuantity')?.value as number | null;
    const unit = control.get('packageUnit')?.value as ProductPackageUnit | null;
    return (quantity !== null) === (unit !== null) ? null : { packagePair: true };
  };
}
