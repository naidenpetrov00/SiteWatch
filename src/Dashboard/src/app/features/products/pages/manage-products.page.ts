import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal
} from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { ProductDialogComponent } from '../components/product-dialog/product-dialog.component';
import {
  DashboardProduct,
  PRODUCT_STATUSES
} from '../models/dashboard-product.models';
import { DashboardProductsService } from '../services/dashboard-products.service';

const PRODUCT_COLUMNS: readonly DataTableColumn<DashboardProduct>[] = [
  {
    key: 'numberId',
    label: 'Number Id',
    sortable: true,
    cellType: 'button',
    filter: { kind: 'number', placeholder: 'Filter Number Id' }
  },
  {
    key: 'id',
    label: 'Id',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Id' }
  },
  {
    key: 'title',
    label: 'Title',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Title' }
  },
  {
    key: 'category',
    label: 'Category',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Category' }
  },
  {
    key: 'brand',
    label: 'Brand',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Brand' }
  },
  {
    key: 'model',
    label: 'Model',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Model' }
  },
  {
    key: 'externalIdentifier',
    label: 'External Identifier',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Identifier' },
    displayFormatter: (_, product) =>
      product.externalIdentifier
        ? [product.externalIdentifierType, product.externalIdentifier]
            .filter(Boolean)
            .join(': ')
        : '—'
  },
  {
    key: 'packageQuantity',
    label: 'Quantity',
    sortable: true,
    filter: { kind: 'number', placeholder: 'Filter Quantity' }
  },
  {
    key: 'packageUnit',
    label: 'Unit',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Unit' }
  },
  {
    key: 'status',
    label: 'Status',
    sortable: true,
    filter: {
      kind: 'select',
      placeholder: 'Filter Status',
      options: PRODUCT_STATUSES.map((status) => ({ label: status, value: status }))
    }
  }
] as const;

@Component({
  selector: 'app-manage-products-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
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
  readonly columns = PRODUCT_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  constructor() {
    effect(() => {
      const state = this.tableState();
      if (state) {
        this.productsService.setTableState(state);
      }
    });

    effect(() => {
      const response = this.productsService.dashboardProductsQuery.data();
      if (response) {
        this.products.set(response.items);
        this.filteredCount.set(response.filteredCount);
        this.totalCount.set(response.totalCount);
      }
    });
  }

  onTableStateChange(state: DataTableState<DashboardProduct>): void {
    this.tableState.set(state);
  }

  openAddProductDialog(): void {
    this.dialog.open(ProductDialogComponent, {
      autoFocus: false,
      width: '72rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: null
    });
  }

  async onNumberIdClick(product: DashboardProduct): Promise<void> {
    try {
      const details = await this.productsService.getProductById(product.id);
      this.dialog.open(ProductDialogComponent, {
        autoFocus: false,
        width: '72rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: details
      });
    } catch {
      // Keep the catalog usable if detail loading fails.
    }
  }
}
