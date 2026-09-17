import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { DashboardRetailersService } from '../services/dashboard-retailers.service';
import { ManageRetailersPage } from './manage-retailers.page';

describe('ManageRetailersPage', () => {
  const retailers = {
    dashboardRetailersQuery: { data: () => undefined, isError: () => false },
    setRetailerStatusMutation: { isPending: () => false },
    setTableState: vi.fn(),
    getRetailerById: vi.fn(),
    setRetailerActive: vi.fn()
  };
  const dialog = { open: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageRetailersPage],
      providers: [
        { provide: DashboardRetailersService, useValue: retailers }
      ]
    }).overrideProvider(MatDialog, { useValue: dialog }).compileComponents();
    vi.clearAllMocks();
  });

  it('forwards table state and opens the add-retailer dialog', async () => {
    const fixture = TestBed.createComponent(ManageRetailersPage);
    const component = fixture.componentInstance;
    const state = { page: { pageIndex: 0, pageSize: 50 } } as never;

    component.onTableStateChange(state);
    dialog.open.mockReturnValue({ afterClosed: () => of(true) });
    await fixture.whenStable();
    await component.openAddRetailerDialog();

    expect(retailers.setTableState).toHaveBeenCalledWith(state);
    expect(dialog.open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ ariaLabel: 'Add Retailer' }));
    expect(component.pageMessage()).toBe('Retailer created successfully.');
  });

  it('opens retrieved details and reports a detail-loading failure', async () => {
    const component = TestBed.createComponent(ManageRetailersPage).componentInstance;
    retailers.getRetailerById.mockResolvedValue({ id: 'retailer-1', displayName: 'Example Store' });
    dialog.open.mockReturnValue({ afterClosed: () => of(false) });

    await component.onCellButtonClick({ row: { id: 'retailer-1', displayName: 'Example Store' }, column: { key: 'displayName' } } as never);
    expect(dialog.open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ data: { id: 'retailer-1', displayName: 'Example Store' } }));

    retailers.getRetailerById.mockRejectedValue(new Error('offline'));
    await component.onCellButtonClick({ row: { id: 'retailer-1', displayName: 'Example Store' }, column: { key: 'displayName' } } as never);
    expect(component.pageError()).toBe('The retailer details could not be loaded.');
  });

  it('confirms and applies a retailer status change', async () => {
    const component = TestBed.createComponent(ManageRetailersPage).componentInstance;
    dialog.open.mockReturnValue({ afterClosed: () => of(true) });
    retailers.setRetailerActive.mockResolvedValue(undefined);

    await component.onCellButtonClick({ row: { id: 'retailer-1', displayName: 'Example Store', isActive: true }, column: { key: 'isActive' } } as never);

    expect(retailers.setRetailerActive).toHaveBeenCalledWith('retailer-1', false);
    expect(component.pageMessage()).toBe('Example Store was deactivated.');
  });
});
