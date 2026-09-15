import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { DashboardProductsService } from '../services/dashboard-products.service';
import { ManageProductsPage } from './manage-products.page';

describe('ManageProductsPage', () => {
  const productsService = {
    dashboardProductsQuery: { data: () => undefined }, setTableState: vi.fn(), getProductById: vi.fn()
  };
  const dialog = { open: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageProductsPage],
      providers: [
        { provide: DashboardProductsService, useValue: productsService },
        provideTanStackQuery(new QueryClient())
      ]
    }).overrideProvider(MatDialog, { useValue: dialog }).compileComponents();
    vi.clearAllMocks();
  });

  it('renders the catalog, forwards table state, and opens the add dialog', async () => {
    const fixture = TestBed.createComponent(ManageProductsPage);
    const state = { page: { pageIndex: 0, pageSize: 50 } } as never;
    fixture.componentInstance.onTableStateChange(state);
    fixture.componentInstance.openAddProductDialog();
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Manage Products');
    expect(productsService.setTableState).toHaveBeenCalledWith(state);
    expect(dialog.open).toHaveBeenCalledTimes(1);
  });

  it('opens retrieved details and absorbs detail-loading failures', async () => {
    productsService.getProductById.mockResolvedValue({ id: 'product-1' });
    const component = TestBed.createComponent(ManageProductsPage).componentInstance;

    await component.onNumberIdClick({ id: 'product-1' } as never);
    expect(dialog.open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ data: { id: 'product-1' } }));

    productsService.getProductById.mockRejectedValue(new Error('offline'));
    await expect(component.onNumberIdClick({ id: 'product-1' } as never)).resolves.toBeUndefined();
  });
});
