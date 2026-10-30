import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { CategoryManagementDialogComponent } from '../components/category-management-dialog/category-management-dialog.component';
import { ProductDialogComponent } from '../components/product-dialog/product-dialog.component';
import { ProductTypeManagementDialogComponent } from '../components/product-type-management-dialog/product-type-management-dialog.component';
import { ProductCatalogCategory, ProductCatalogProductType } from '../models/product-catalog.models';
import { DashboardProduct, PRODUCT_PACKAGE_UNIT_OPTIONS, PRODUCT_STATUSES } from '../models/dashboard-product.models';
import { ProductCatalogService } from '../services/product-catalog.service';
import { DashboardProductsService } from '../services/dashboard-products.service';

const PRODUCT_COLUMNS: readonly DataTableColumn<DashboardProduct>[] = [
  { key: 'numberId', label: 'Number Id', sortable: true, cellType: 'button', filter: { kind: 'number', placeholder: 'Filter Number Id' } },
  { key: 'id', label: 'Id', sortable: true, filter: { kind: 'text', placeholder: 'Filter Id' } },
  {
    key: 'title', label: 'Title', sortable: true, cellType: 'internal-link',
    filter: { kind: 'text', placeholder: 'Filter Title' },
    linkRouteAccessor: (product) => ['/manage-products', product.id],
    ariaLabelAccessor: (product) => 'Open Product ' + product.numberId + ', ' + product.title
  },
  { key: 'productTypeName', label: 'Product Type', sortable: true, sortKey: 'productTypeName' },
  { key: 'categoryName', label: 'Category', sortable: true, sortKey: 'categoryName' },
  { key: 'brand', label: 'Brand', sortable: true, filter: { kind: 'text', placeholder: 'Filter Brand' } },
  { key: 'model', label: 'Model', sortable: true, filter: { kind: 'text', placeholder: 'Filter Model' } },
  { key: 'packageQuantity', label: 'Quantity', sortable: true, filter: { kind: 'number', placeholder: 'Filter Quantity' } },
  {
    key: 'packageUnit', label: 'Unit', sortable: true,
    filter: { kind: 'select', placeholder: 'Filter Unit', options: PRODUCT_PACKAGE_UNIT_OPTIONS },
    displayFormatter: (value) => PRODUCT_PACKAGE_UNIT_OPTIONS.find((unit) => unit.value === value)?.label ?? '—'
  },
  {
    key: 'status', label: 'Status', sortable: true,
    filter: { kind: 'select', placeholder: 'Filter Status', options: PRODUCT_STATUSES.map((status) => ({ label: status, value: status })) }
  }
] as const;

@Component({
  selector: 'app-manage-products-page',
  imports: [ReactiveFormsModule, ActionButtonComponent, DataTableComponent, MatAutocompleteModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatMenuModule],
  templateUrl: './manage-products.page.html',
  styleUrl: './manage-products.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageProductsPage {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly productsService = inject(DashboardProductsService);
  private readonly catalogService = inject(ProductCatalogService);
  private readonly dialog = inject(MatDialog);
  private categorySearchRevision = 0;
  private productTypeSearchRevision = 0;

  readonly products = signal<readonly DashboardProduct[]>([]);
  readonly filteredCount = signal(0);
  readonly totalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardProduct> | null>(null);
  readonly categoryResults = signal<readonly ProductCatalogCategory[]>([]);
  readonly productTypeResults = signal<readonly ProductCatalogProductType[]>([]);
  readonly categoryLoading = signal(false);
  readonly productTypeLoading = signal(false);
  readonly catalogLookupError = signal<string | null>(null);
  readonly categoryFilterControl = this.formBuilder.control<string | ProductCatalogCategory | null>(null);
  readonly productTypeFilterControl = this.formBuilder.control<string | ProductCatalogProductType | null>(null);
  readonly catalogFilterRevision = this.productsService.catalogFilterRevision;
  readonly columns = PRODUCT_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;
  readonly displayCategory = (value: string | ProductCatalogCategory | null): string => typeof value === 'string' ? value : value?.name ?? '';
  readonly displayProductType = (value: string | ProductCatalogProductType | null): string =>
    typeof value === 'string' || value === null ? value ?? '' : `${value.name} · ${value.categoryName}`;

  constructor() {
    effect(() => {
      const state = this.tableState();
      if (state) this.productsService.setTableState(state);
    });
    effect(() => {
      const response = this.productsService.dashboardProductsQuery.data();
      if (!response) return;
      this.products.set(response.items);
      this.filteredCount.set(response.filteredCount);
      this.totalCount.set(response.totalCount);
    });

    this.categoryFilterControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value !== 'string') return;
      this.categorySearchRevision += 1;
      this.productTypeSearchRevision += 1;
      this.clearCategoryLookup();
      this.clearProductTypeLookup();
      this.productTypeFilterControl.setValue(null, { emitEvent: false });
      this.productsService.setCatalogFilters({ categoryId: null, productTypeId: null });
    });
    this.categoryFilterControl.valueChanges.pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value === 'string') void this.searchCategories(value, this.categorySearchRevision);
    });
    this.productTypeFilterControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value !== 'string') return;
      this.productTypeSearchRevision += 1;
      this.clearProductTypeLookup();
      this.productsService.setCatalogFilters({ productTypeId: null });
    });
    this.productTypeFilterControl.valueChanges.pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value === 'string') void this.searchProductTypes(value, this.productTypeSearchRevision);
    });
  }

  onTableStateChange(state: DataTableState<DashboardProduct>): void { this.tableState.set(state); }

  onCategorySelected(event: MatAutocompleteSelectedEvent): void {
    const category = event.option.value as ProductCatalogCategory;
    this.categorySearchRevision += 1;
    this.productTypeSearchRevision += 1;
    this.clearCategoryLookup();
    this.clearProductTypeLookup();
    this.categoryFilterControl.setValue(category, { emitEvent: false });
    this.productTypeFilterControl.setValue(null, { emitEvent: false });
    this.productsService.setCatalogFilters({ categoryId: category.id, productTypeId: null });
  }

  onProductTypeSelected(event: MatAutocompleteSelectedEvent): void {
    const productType = event.option.value as ProductCatalogProductType;
    this.categorySearchRevision += 1;
    this.productTypeSearchRevision += 1;
    this.clearCategoryLookup();
    this.clearProductTypeLookup();
    this.productTypeFilterControl.setValue(productType, { emitEvent: false });
    this.categoryFilterControl.setValue({ id: productType.categoryId, name: productType.categoryName }, { emitEvent: false });
    this.productsService.setCatalogFilters({ categoryId: productType.categoryId, productTypeId: productType.id });
  }

  clearCatalogFilters(): void {
    this.categorySearchRevision += 1;
    this.productTypeSearchRevision += 1;
    this.clearCategoryLookup();
    this.clearProductTypeLookup();
    this.categoryFilterControl.setValue(null, { emitEvent: false });
    this.productTypeFilterControl.setValue(null, { emitEvent: false });
    this.productsService.setCatalogFilters({ categoryId: null, productTypeId: null });
  }

  openAddProductDialog(): void {
    this.dialog.open(ProductDialogComponent, { autoFocus: false, restoreFocus: true, width: '72rem', maxWidth: 'calc(100vw - 2rem)', data: null });
  }

  openCategoryManagement(): void {
    this.dialog.open(CategoryManagementDialogComponent, { autoFocus: false, restoreFocus: true, width: '48rem', maxWidth: 'calc(100vw - 2rem)' });
  }

  openProductTypeManagement(): void {
    this.dialog.open(ProductTypeManagementDialogComponent, { autoFocus: false, restoreFocus: true, width: '72rem', maxWidth: 'calc(100vw - 2rem)' });
  }

  async onCellButtonClicked(event: { row: DashboardProduct; column: DataTableColumn<DashboardProduct> }): Promise<void> {
    if (event.column.key !== 'numberId') return;
    try {
      const product = await this.productsService.getProductById(event.row.id);
      this.dialog.open(ProductDialogComponent, { autoFocus: false, restoreFocus: true, width: '72rem', maxWidth: 'calc(100vw - 2rem)', data: product });
    } catch {
      // Keep the table usable if product details cannot be loaded.
    }
  }

  private async searchCategories(searchTerm: string, revision: number): Promise<void> {
    const normalized = searchTerm.trim();
    if (!normalized) {
      this.clearCategoryLookup();
      return;
    }
    this.categoryLoading.set(true);
    this.catalogLookupError.set(null);
    try {
      const response = await this.catalogService.getCategories({ searchTerm: normalized, pageSize: 20 });
      if (revision === this.categorySearchRevision) this.categoryResults.set(response.items);
    } catch {
      if (revision === this.categorySearchRevision) {
        this.categoryResults.set([]);
        this.catalogLookupError.set('Categories could not be searched. Try again.');
      }
    } finally {
      if (revision === this.categorySearchRevision) this.categoryLoading.set(false);
    }
  }

  private async searchProductTypes(searchTerm: string, revision: number): Promise<void> {
    const normalized = searchTerm.trim();
    if (!normalized) {
      this.clearProductTypeLookup();
      return;
    }
    this.productTypeLoading.set(true);
    this.catalogLookupError.set(null);
    try {
      const category = this.categoryFilterControl.value;
      const response = await this.catalogService.getProductTypes({
        searchTerm: normalized,
        categoryId: typeof category === 'object' && category ? category.id : null,
        pageSize: 20
      });
      if (revision === this.productTypeSearchRevision) this.productTypeResults.set(response.items);
    } catch {
      if (revision === this.productTypeSearchRevision) {
        this.productTypeResults.set([]);
        this.catalogLookupError.set('Product Types could not be searched. Try again.');
      }
    } finally {
      if (revision === this.productTypeSearchRevision) this.productTypeLoading.set(false);
    }
  }

  private clearCategoryLookup(): void {
    this.categoryResults.set([]);
    this.categoryLoading.set(false);
    this.catalogLookupError.set(null);
  }

  private clearProductTypeLookup(): void {
    this.productTypeResults.set([]);
    this.productTypeLoading.set(false);
    this.catalogLookupError.set(null);
  }
}
