import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { CatalogSelectorComponent } from '../components/catalog-selector/catalog-selector.component';
import { CategoryManagementDialogComponent } from '../components/category-management-dialog/category-management-dialog.component';
import { ProductDialogComponent } from '../components/product-dialog/product-dialog.component';
import { ProductTypeManagementDialogComponent } from '../components/product-type-management-dialog/product-type-management-dialog.component';
import { ProductCatalogCategory, ProductCatalogProductType } from '../models/product-catalog.models';
import { DashboardProduct, PRODUCT_PACKAGE_UNIT_OPTIONS, PRODUCT_STATUSES } from '../models/dashboard-product.models';
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
  imports: [ReactiveFormsModule, ActionButtonComponent, CatalogSelectorComponent, DataTableComponent, MatButtonModule, MatDialogModule, MatMenuModule],
  templateUrl: './manage-products.page.html',
  styleUrl: './manage-products.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageProductsPage {
  private readonly productsService = inject(DashboardProductsService);
  private readonly dialog = inject(MatDialog);

  readonly products = signal<readonly DashboardProduct[]>([]);
  readonly filteredCount = signal(0);
  readonly totalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardProduct> | null>(null);
  readonly categoryFilterControl = new FormControl<ProductCatalogCategory | null>(null);
  readonly productTypeFilterControl = new FormControl<ProductCatalogProductType | null>(null);
  readonly selectedCategoryId = signal<string | null>(null);
  readonly catalogFilterRevision = this.productsService.catalogFilterRevision;
  readonly columns = PRODUCT_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;
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

    this.categoryFilterControl.valueChanges.subscribe((category) => {
      this.selectedCategoryId.set(category?.id ?? null);
      this.productTypeFilterControl.setValue(null, { emitEvent: false });
      this.productsService.setCatalogFilters({ categoryId: category?.id ?? null, productTypeId: null });
    });
    this.productTypeFilterControl.valueChanges.subscribe((productType) => {
      if (!productType) {
        this.productsService.setCatalogFilters({ productTypeId: null });
        return;
      }
      const category = { id: productType.categoryId, name: productType.categoryName };
      this.categoryFilterControl.setValue(category, { emitEvent: false });
      this.selectedCategoryId.set(category.id);
      this.productsService.setCatalogFilters({ categoryId: productType.categoryId, productTypeId: productType.id });
    });
  }

  onTableStateChange(state: DataTableState<DashboardProduct>): void { this.tableState.set(state); }

  clearCatalogFilters(): void {
    this.categoryFilterControl.setValue(null, { emitEvent: false });
    this.productTypeFilterControl.setValue(null, { emitEvent: false });
    this.selectedCategoryId.set(null);
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

}
