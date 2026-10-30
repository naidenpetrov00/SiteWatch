import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import {
  ProductCatalogCategory,
  ProductCatalogProductType
} from '../../models/product-catalog.models';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';
import {
  CatalogConfirmDialogComponent,
  CatalogConfirmDialogData
} from '../catalog-confirm-dialog/catalog-confirm-dialog.component';
import {
  ProductTypeEditorDialogComponent,
  ProductTypeEditorDialogData
} from '../product-type-editor-dialog/product-type-editor-dialog.component';

@Component({
  selector: 'app-product-type-management-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatTableModule
  ],
  templateUrl: './product-type-management-dialog.component.html',
  styleUrl: './product-type-management-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductTypeManagementDialogComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly catalogService = inject(ProductCatalogService);
  private categorySearchRevision = 0;

  readonly displayedColumns = ['name', 'categoryName', 'actions'];
  readonly nameSearchControl = this.formBuilder.nonNullable.control('');
  readonly categoryFilterControl = this.formBuilder.control<
    string | ProductCatalogCategory | null
  >(null);
  readonly searchTerm = signal('');
  readonly categoryId = signal<string | null>(null);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(25);
  readonly categoryResults = signal<readonly ProductCatalogCategory[]>([]);
  readonly categoryLoading = signal(false);
  readonly categoryLookupError = signal<string | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly deletingId = signal<string | null>(null);
  readonly queryState = computed(() => ({
    searchTerm: this.searchTerm(),
    categoryId: this.categoryId(),
    pageIndex: this.pageIndex(),
    pageSize: this.pageSize()
  }));
  readonly productTypesQuery = injectQuery(() => ({
    queryKey: ['product-catalog', 'product-types', this.queryState()] as const,
    queryFn: () => this.catalogService.getProductTypes(this.queryState())
  }));
  readonly displayCategory = (value: string | ProductCatalogCategory | null): string =>
    typeof value === 'string' ? value : value?.name ?? '';

  constructor() {
    this.nameSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((searchTerm) => {
        this.searchTerm.set(searchTerm.trim());
        this.pageIndex.set(0);
      });

    this.categoryFilterControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') return;
        this.categoryId.set(null);
        this.categorySearchRevision += 1;
        this.pageIndex.set(0);
        this.clearCategoryLookup();
      });
    this.categoryFilterControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') void this.searchCategories(value, this.categorySearchRevision);
      });
  }

  onCategoryFilterSelected(event: MatAutocompleteSelectedEvent): void {
    const category = event.option.value as ProductCatalogCategory;
    this.categorySearchRevision += 1;
    this.clearCategoryLookup();
    this.categoryFilterControl.setValue(category, { emitEvent: false });
    this.categoryId.set(category.id);
    this.pageIndex.set(0);
  }

  clearCategoryFilter(): void {
    this.categorySearchRevision += 1;
    this.clearCategoryLookup();
    this.categoryFilterControl.setValue(null, { emitEvent: false });
    this.categoryId.set(null);
    this.pageIndex.set(0);
  }

  async createProductType(): Promise<void> {
    await this.openEditor(null);
  }

  async editProductType(productType: ProductCatalogProductType): Promise<void> {
    await this.openEditor(productType);
  }

  async deleteProductType(productType: ProductCatalogProductType): Promise<void> {
    if (this.deletingId()) return;
    const confirmRef = this.dialog.open<
      CatalogConfirmDialogComponent,
      CatalogConfirmDialogData,
      boolean
    >(CatalogConfirmDialogComponent, {
      autoFocus: 'button',
      restoreFocus: true,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        title: 'Delete Product Type',
        message: `Delete ${productType.name} in ${productType.categoryName}? This is allowed only when it is unused.`,
        confirmLabel: 'Delete Product Type'
      }
    });
    if (!(await firstValueFrom(confirmRef.afterClosed()))) return;

    this.deletingId.set(productType.id);
    this.pageError.set(null);
    try {
      await this.catalogService.deleteProductType(productType.id);
      await this.productTypesQuery.refetch();
    } catch (error) {
      this.pageError.set(getProductCatalogError(error, 'The Product Type could not be deleted.'));
      if ((error as { status?: number }).status === 404) await this.productTypesQuery.refetch();
    } finally {
      this.deletingId.set(null);
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }

  async retry(): Promise<void> {
    this.pageError.set(null);
    await this.productTypesQuery.refetch();
  }

  private async openEditor(productType: ProductCatalogProductType | null): Promise<void> {
    this.pageError.set(null);
    const dialogRef = this.dialog.open<
      ProductTypeEditorDialogComponent,
      ProductTypeEditorDialogData,
      ProductCatalogProductType | null
    >(ProductTypeEditorDialogComponent, {
      autoFocus: 'input[formControlName="name"]',
      restoreFocus: true,
      width: '42rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { productType }
    });
    if (await firstValueFrom(dialogRef.afterClosed())) await this.productTypesQuery.refetch();
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
      if (revision === this.categorySearchRevision) this.categoryResults.set(response.items);
    } catch {
      if (revision === this.categorySearchRevision) {
        this.categoryResults.set([]);
        this.categoryLookupError.set('Categories could not be searched. Try again.');
      }
    } finally {
      if (revision === this.categorySearchRevision) this.categoryLoading.set(false);
    }
  }

  private clearCategoryLookup(): void {
    this.categoryResults.set([]);
    this.categoryLoading.set(false);
    this.categoryLookupError.set(null);
  }
}
