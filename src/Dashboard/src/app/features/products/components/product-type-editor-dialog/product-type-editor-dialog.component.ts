import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  ViewChild,
  inject,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  ProductCatalogCategory,
  ProductCatalogProductType
} from '../../models/product-catalog.models';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';
import { trimmedRequiredValidator } from '../../utils/product-catalog.validators';
import {
  CategoryEditorDialogComponent,
  CategoryEditorDialogData
} from '../category-editor-dialog/category-editor-dialog.component';

export interface ProductTypeEditorDialogData {
  productType: ProductCatalogProductType | null;
}

@Component({
  selector: 'app-product-type-editor-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './product-type-editor-dialog.component.html',
  styleUrl: './product-type-editor-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductTypeEditorDialogComponent {
  @ViewChild('categoryInput') private categoryInput?: ElementRef<HTMLInputElement>;
  readonly data = inject<ProductTypeEditorDialogData>(MAT_DIALOG_DATA);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly catalogService = inject(ProductCatalogService);
  private readonly dialogRef = inject(
    MatDialogRef<ProductTypeEditorDialogComponent, ProductCatalogProductType | null>
  );
  private searchRevision = 0;

  readonly isEdit = this.data.productType !== null;
  readonly formId = this.isEdit ? 'edit-product-type-form' : 'create-product-type-form';
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly categoryResults = signal<readonly ProductCatalogCategory[]>([]);
  readonly categoryLoading = signal(false);
  readonly categoryLookupError = signal<string | null>(null);
  readonly selectedCategory = signal<ProductCatalogCategory | null>(
    this.data.productType
      ? {
          id: this.data.productType.categoryId,
          name: this.data.productType.categoryName
        }
      : null
  );
  readonly categoryControl = this.formBuilder.control<string | ProductCatalogCategory | null>(
    this.selectedCategory()
  );
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control(this.data.productType?.name ?? '', [
      trimmedRequiredValidator(),
      Validators.maxLength(200)
    ]),
    categoryId: this.formBuilder.nonNullable.control(
      this.data.productType?.categoryId ?? '',
      [Validators.required]
    )
  });

  readonly categoryChanged = () =>
    this.isEdit && this.form.controls.categoryId.value !== this.data.productType?.categoryId;
  readonly displayCategory = (value: string | ProductCatalogCategory | null): string =>
    typeof value === 'string' ? value : value?.name ?? '';

  constructor() {
    this.categoryControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') return;
        this.form.controls.categoryId.setValue('');
        this.selectedCategory.set(null);
        this.searchRevision += 1;
        this.clearCategoryLookup();
      });

    this.categoryControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') void this.searchCategories(value, this.searchRevision);
      });
  }

  onCategorySelected(event: MatAutocompleteSelectedEvent): void {
    this.selectCategory(event.option.value as ProductCatalogCategory);
  }

  async createCategory(): Promise<void> {
    if (this.saving()) return;
    const dialogRef = this.dialog.open<
      CategoryEditorDialogComponent,
      CategoryEditorDialogData,
      ProductCatalogCategory | null
    >(CategoryEditorDialogComponent, {
      autoFocus: 'input[formControlName="name"]',
      restoreFocus: true,
      width: '30rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { category: null }
    });
    const category = await firstValueFrom(dialogRef.afterClosed());
    if (category) {
      this.selectCategory(category);
      setTimeout(() => this.categoryInput?.nativeElement.focus());
    }
  }

  close(): void {
    if (!this.saving()) this.dialogRef.close(null);
  }

  async save(): Promise<void> {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.categoryControl.markAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    try {
      const request = {
        name: this.form.controls.name.value.trim(),
        categoryId: this.form.controls.categoryId.value
      };
      const productType = this.data.productType
        ? await this.catalogService.updateProductType(this.data.productType.id, request)
        : await this.catalogService.createProductType(request);
      this.dialogRef.close(productType);
    } catch (error) {
      this.saveError.set(getProductCatalogError(error, 'The Product Type could not be saved.'));
    } finally {
      this.saving.set(false);
    }
  }

  private selectCategory(category: ProductCatalogCategory): void {
    this.searchRevision += 1;
    this.clearCategoryLookup();
    this.selectedCategory.set(category);
    this.categoryControl.setValue(category, { emitEvent: false });
    this.form.controls.categoryId.setValue(category.id);
  }

  private async searchCategories(searchTerm: string, revision: number): Promise<void> {
    const normalized = searchTerm.trim();
    if (!normalized) {
      this.clearCategoryLookup();
      return;
    }

    this.categoryLoading.set(true);
    this.categoryLookupError.set(null);
    try {
      const response = await this.catalogService.getCategories({
        searchTerm: normalized,
        pageSize: 20
      });
      if (revision === this.searchRevision) this.categoryResults.set(response.items);
    } catch {
      if (revision === this.searchRevision) {
        this.categoryResults.set([]);
        this.categoryLookupError.set('Categories could not be searched. Try again.');
      }
    } finally {
      if (revision === this.searchRevision) this.categoryLoading.set(false);
    }
  }

  private clearCategoryLookup(): void {
    this.categoryResults.set([]);
    this.categoryLoading.set(false);
    this.categoryLookupError.set(null);
  }
}
