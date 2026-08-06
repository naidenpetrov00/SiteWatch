import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { vi } from 'vitest';
import { DashboardInvoicesService } from '../services/dashboard-invoices.service';

import { InvoicesPage } from './invoices.page';

describe('InvoicesPage', () => {
  const invoicesService = {
    dashboardInvoicesQuery: { data: () => undefined },
    setTableState: vi.fn()
  };
  const dialog = { open: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoicesPage],
      providers: [
        { provide: DashboardInvoicesService, useValue: invoicesService },
        { provide: MatDialog, useValue: dialog }
      ]
    }).compileComponents();
    dialog.open.mockReset();
  });

  it('renders the invoices heading', async () => {
    const fixture = TestBed.createComponent(InvoicesPage);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Invoices');
  });

  it('treats incomplete invoices as error rows', () => {
    const fixture = TestBed.createComponent(InvoicesPage);

    expect(fixture.componentInstance.isIncompleteInvoice({ isComplete: false } as never)).toBe(true);
    expect(fixture.componentInstance.isIncompleteInvoice({ isComplete: true } as never)).toBe(false);
  });

  it('opens the invoice dialog only when the number column is selected', () => {
    const fixture = TestBed.createComponent(InvoicesPage);
    const invoice = { id: 'invoice-1', numberId: 42 } as never;

    fixture.componentInstance.onCellButtonClicked({ row: invoice, column: { key: 'id' } as never });
    expect(dialog.open).not.toHaveBeenCalled();

    fixture.componentInstance.onCellButtonClicked({ row: invoice, column: { key: 'numberId' } as never });
    expect(dialog.open).toHaveBeenCalledTimes(1);
  });
});
