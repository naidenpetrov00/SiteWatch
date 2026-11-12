import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { ProductCatalogBrand } from '../../../../shared/product-catalog/product-catalog.models';
import { ProductCatalogLookupService } from '../../../../shared/product-catalog/product-catalog-lookup.service';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';
import { trimmedRequiredValidator } from '../../utils/product-catalog.validators';

export interface ProductBrandEditorDialogData {
  productBrand: ProductCatalogBrand | null;
}

@Component({
  selector: 'app-product-brand-editor-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './product-brand-editor-dialog.component.html',
  styleUrl: './product-brand-editor-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductBrandEditorDialogComponent {
  readonly data = inject<ProductBrandEditorDialogData>(MAT_DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogLookupService = inject(ProductCatalogLookupService);
  private readonly catalogService = inject(ProductCatalogService);
  private readonly dialogRef = inject(
    MatDialogRef<ProductBrandEditorDialogComponent, ProductCatalogBrand | null>
  );
  private searchRevision = 0;

  readonly isEdit = this.data.productBrand !== null;
  readonly formId = this.isEdit ? 'edit-product-brand-form' : 'create-product-brand-form';
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly matches = signal<readonly ProductCatalogBrand[]>([]);
  readonly enteredName = signal(this.data.productBrand?.name ?? '');
  readonly exactDuplicate = computed(() => {
    const normalized = normalizeName(this.enteredName());
    if (!normalized) return false;
    return this.matches().some((brand) =>
      brand.id !== this.data.productBrand?.id && normalizeName(brand.name) === normalized);
  });
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control(this.data.productBrand?.name ?? '', [
      trimmedRequiredValidator(),
      Validators.maxLength(100)
    ])
  });

  constructor() {
    this.form.controls.name.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.enteredName.set(value);
        const revision = ++this.searchRevision;
        const searchTerm = value.trim();
        if (!searchTerm) {
          this.matches.set([]);
          return;
        }
        void this.loadMatches(searchTerm, revision);
      });
  }

  close(): void {
    if (!this.saving()) this.dialogRef.close(null);
  }

  async save(): Promise<void> {
    if (this.saving() || this.exactDuplicate()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    try {
      const request = { name: this.form.controls.name.value.trim() };
      const brand = this.data.productBrand
        ? await this.catalogService.updateProductBrand(this.data.productBrand.id, request)
        : await this.catalogService.createProductBrand(request);
      this.dialogRef.close(brand);
    } catch (error) {
      this.saveError.set(getProductCatalogError(error, 'The Product Brand could not be saved.'));
    } finally {
      this.saving.set(false);
    }
  }

  private async loadMatches(searchTerm: string, revision: number): Promise<void> {
    try {
      const response = await this.catalogLookupService.getProductBrands({
        searchTerm,
        pageSize: 5
      });
      if (revision === this.searchRevision) this.matches.set(response.items);
    } catch {
      if (revision === this.searchRevision) this.matches.set([]);
    }
  }
}

function normalizeName(value: string): string {
  return value.trim().replace(/\s+/g, ' ').toUpperCase();
}
