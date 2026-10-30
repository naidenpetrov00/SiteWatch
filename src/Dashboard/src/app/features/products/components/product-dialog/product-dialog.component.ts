import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormControl, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DialogWizardTabsComponent } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.component';
import { DialogWizardTabDefinition } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.types';
import { ProductTypeEditorDialogComponent, ProductTypeEditorDialogData } from '../product-type-editor-dialog/product-type-editor-dialog.component';
import { ProductCatalogProductType } from '../../models/product-catalog.models';
import { CreateDashboardProductRequest, DashboardProductDetails, PRODUCT_PACKAGE_UNIT_OPTIONS, PRODUCT_STATUSES, ProductPackageUnit, ProductStatus } from '../../models/dashboard-product.models';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { DashboardProductsService } from '../../services/dashboard-products.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';

export type ProductDialogResult = true | { createdProductId: string };
type ProductDialogTabId = 'details' | 'search';
type SearchCollectionName = 'alternativeSearchPhrases' | 'requiredKeywords' | 'excludedKeywords';

const PRODUCT_DIALOG_TABS = [
  { id: 'details', label: 'Product Details' },
  { id: 'search', label: 'Search Configuration' }
] as const satisfies readonly DialogWizardTabDefinition[];

@Component({
  selector: 'app-product-dialog',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, DialogActionBarComponent, DialogShellComponent, DialogWizardTabsComponent],
  templateUrl: './product-dialog.component.html',
  styleUrl: './product-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDialogComponent {
  @ViewChild('productTypeInput') private productTypeInput?: ElementRef<HTMLInputElement>;

  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly dialogRef = inject(MatDialogRef<ProductDialogComponent, ProductDialogResult>);
  private readonly productsService = inject(DashboardProductsService);
  private readonly catalogService = inject(ProductCatalogService);
  private readonly product = inject<DashboardProductDetails | null>(MAT_DIALOG_DATA, { optional: true });
  private searchRevision = 0;

  readonly statuses = PRODUCT_STATUSES;
  readonly packageUnits = PRODUCT_PACKAGE_UNIT_OPTIONS;
  readonly tabs = PRODUCT_DIALOG_TABS;
  readonly selectedTabId = signal<ProductDialogTabId>('details');
  readonly productTypeResults = signal<readonly ProductCatalogProductType[]>([]);
  readonly productTypeLoading = signal(false);
  readonly productTypeLookupError = signal<string | null>(null);
  readonly selectedProductType = signal<ProductCatalogProductType | null>(
    this.product ? {
      id: this.product.productTypeId,
      name: this.product.productTypeName,
      categoryId: this.product.categoryId,
      categoryName: this.product.categoryName
    } : null
  );
  readonly productTypeControl = this.formBuilder.control<string | ProductCatalogProductType | null>(
    this.selectedProductType()
  );
  readonly saveError = signal<string | null>(null);
  readonly formId = this.product ? 'edit-product-dialog-form' : 'add-product-dialog-form';
  readonly dialogTitle = this.product ? 'Edit Product' : 'Add Product';
  readonly dialogSubtitle = this.product
    ? `Update #${this.product.numberId} ${this.product.title}.`
    : 'Enter the Product identity and future retailer-search configuration.';
  readonly submitLabel = this.product ? 'Save Product' : 'Add Product';
  readonly productForm = this.formBuilder.group({
    title: this.formBuilder.nonNullable.control(this.product?.title ?? '', [Validators.required, Validators.maxLength(200)]),
    description: this.formBuilder.nonNullable.control(this.product?.description ?? '', [Validators.maxLength(2000)]),
    brand: this.formBuilder.nonNullable.control(this.product?.brand ?? '', [Validators.maxLength(100)]),
    model: this.formBuilder.nonNullable.control(this.product?.model ?? '', [Validators.maxLength(100)]),
    packageQuantity: this.formBuilder.control<number | null>(this.product?.packageQuantity ?? null, [Validators.min(Number.MIN_VALUE)]),
    packageUnit: this.formBuilder.control<ProductPackageUnit | null>(this.product?.packageUnit ?? null),
    productTypeId: this.formBuilder.nonNullable.control(this.product?.productTypeId ?? '', [Validators.required]),
    status: this.formBuilder.nonNullable.control<ProductStatus>(this.product?.status ?? 'Active', [Validators.required]),
    primarySearchPhrase: this.formBuilder.nonNullable.control(this.product?.primarySearchPhrase ?? '', [Validators.maxLength(250)]),
    alternativeSearchPhrases: this.createSearchCollection(this.product?.alternativeSearchPhrases),
    requiredKeywords: this.createSearchCollection(this.product?.requiredKeywords),
    excludedKeywords: this.createSearchCollection(this.product?.excludedKeywords)
  }, { validators: [packagePairValidator()] });

  readonly isSaving = (): boolean => this.product
    ? this.productsService.updateProductMutation.isPending()
    : this.productsService.createProductMutation.isPending();
  readonly displayProductType = (value: string | ProductCatalogProductType | null): string =>
    typeof value === 'string' || value === null ? value ?? '' : `${value.name} · ${value.categoryName}`;

  constructor() {
    this.productTypeControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value !== 'string') return;
      this.searchRevision += 1;
      this.selectedProductType.set(null);
      this.clearProductTypeLookup();
      this.productForm.controls.productTypeId.setValue('');
    });
    this.productTypeControl.valueChanges.pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value === 'string') void this.searchProductTypes(value, this.searchRevision);
    });
  }

  setSelectedTab(tabId: string): void {
    if (this.tabs.some((tab) => tab.id === tabId)) this.selectedTabId.set(tabId as ProductDialogTabId);
  }

  onProductTypeSelected(event: MatAutocompleteSelectedEvent): void {
    this.selectProductType(event.option.value as ProductCatalogProductType);
  }

  async createProductType(): Promise<void> {
    if (this.isSaving()) return;
    const dialogRef = this.dialog.open<ProductTypeEditorDialogComponent, ProductTypeEditorDialogData, ProductCatalogProductType | null>(ProductTypeEditorDialogComponent, {
      autoFocus: 'input[formControlName="name"]',
      restoreFocus: true,
      width: '42rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { productType: null }
    });
    const productType = await firstValueFrom(dialogRef.afterClosed());
    if (!productType) return;
    this.selectProductType(productType);
    setTimeout(() => this.productTypeInput?.nativeElement.focus());
  }

  addSearchEntry(collectionName: SearchCollectionName): void {
    const collection = this.productForm.controls[collectionName];
    if (collection.length < 20) collection.push(this.createSearchEntry());
  }

  removeSearchEntry(collectionName: SearchCollectionName, index: number): void {
    this.productForm.controls[collectionName].removeAt(index);
  }

  effectivePrimarySearchPhrase(): string {
    const customPhrase = this.productForm.controls.primarySearchPhrase.value.trim();
    if (customPhrase) return customPhrase;
    return [this.productForm.controls.title.value, this.productForm.controls.brand.value, this.productForm.controls.model.value]
      .map((value) => value.trim()).filter(Boolean).join(' ');
  }

  closeDialog(): void {
    if (!this.isSaving()) this.dialogRef.close();
  }

  async submitProduct(): Promise<void> {
    if (this.isSaving()) return;
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      this.productTypeControl.markAsTouched();
      return;
    }
    this.saveError.set(null);
    const request = this.toRequest();
    try {
      if (this.product) {
        await this.productsService.updateProduct({ id: this.product.id, ...request });
        this.dialogRef.close(true);
      } else {
        const created = await this.productsService.createProduct(request);
        this.dialogRef.close({ createdProductId: created.id });
      }
    } catch (error) {
      this.saveError.set(getProductCatalogError(error, 'The Product could not be saved.'));
    }
  }

  private selectProductType(productType: ProductCatalogProductType): void {
    this.searchRevision += 1;
    this.clearProductTypeLookup();
    this.selectedProductType.set(productType);
    this.productTypeControl.setValue(productType, { emitEvent: false });
    this.productForm.controls.productTypeId.setValue(productType.id);
  }

  private async searchProductTypes(searchTerm: string, revision: number): Promise<void> {
    const normalized = searchTerm.trim();
    if (!normalized) {
      this.clearProductTypeLookup();
      return;
    }
    this.productTypeLoading.set(true);
    this.productTypeLookupError.set(null);
    try {
      const response = await this.catalogService.getProductTypes({ searchTerm: normalized, pageSize: 20 });
      if (revision === this.searchRevision) this.productTypeResults.set(response.items);
    } catch {
      if (revision === this.searchRevision) {
        this.productTypeResults.set([]);
        this.productTypeLookupError.set('Product Types could not be searched. Try again.');
      }
    } finally {
      if (revision === this.searchRevision) this.productTypeLoading.set(false);
    }
  }

  private toRequest(): CreateDashboardProductRequest {
    const value = this.productForm.getRawValue();
    return {
      title: value.title.trim(), description: this.emptyToNull(value.description), brand: this.emptyToNull(value.brand),
      model: this.emptyToNull(value.model), packageQuantity: value.packageQuantity, packageUnit: value.packageUnit,
      productTypeId: value.productTypeId, status: value.status, primarySearchPhrase: this.emptyToNull(value.primarySearchPhrase),
      alternativeSearchPhrases: this.normalizeCollection(value.alternativeSearchPhrases),
      requiredKeywords: this.normalizeCollection(value.requiredKeywords),
      excludedKeywords: this.normalizeCollection(value.excludedKeywords)
    };
  }

  private clearProductTypeLookup(): void {
    this.productTypeResults.set([]);
    this.productTypeLoading.set(false);
    this.productTypeLookupError.set(null);
  }

  private createSearchCollection(values: readonly string[] = []): FormArray<FormControl<string>> {
    return this.formBuilder.array(values.map((value) => this.createSearchEntry(value)));
  }

  private createSearchEntry(value = ''): FormControl<string> {
    return this.formBuilder.nonNullable.control(value, [Validators.required, Validators.maxLength(200)]);
  }

  private normalizeCollection(values: readonly string[]): string[] {
    const normalizedValues: string[] = [];
    for (const value of values) {
      const normalizedValue = value.trim().replace(/\s+/g, ' ');
      if (normalizedValue && !normalizedValues.some((existing) => existing.toLowerCase() === normalizedValue.toLowerCase())) normalizedValues.push(normalizedValue);
    }
    return normalizedValues;
  }

  private emptyToNull(value: string): string | null { return value.trim() || null; }
}

function packagePairValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const quantity = control.get('packageQuantity')?.value as number | null;
    const unit = control.get('packageUnit')?.value as ProductPackageUnit | null;
    return (quantity !== null) === (unit !== null) ? null : { packagePair: true };
  };
}
