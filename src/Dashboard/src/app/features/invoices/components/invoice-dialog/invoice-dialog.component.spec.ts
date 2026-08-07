import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardPersonsService } from '../../../persons/services/dashboard-persons.service';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardInvoicesService } from '../../services/dashboard-invoices.service';
import { DashboardInvoice } from '../../models/dashboard-invoice.model';
import { InvoiceDialogComponent } from './invoice-dialog.component';

const invoice: DashboardInvoice = {
  id: 'invoice-1', numberId: 42, isComplete: true,
  supplierId: '11111111-1111-1111-1111-111111111111', supplierDisplayLabel: 'Acme Ltd', submittedFromSiteName: 'North',
  invoiceNumber: 'INV-42', date: '2026-01-02T00:00:00.000Z', created: '2026-01-01T00:00:00.000Z',
  taxIdentifier: 'BG123', address: '1 Main Street', email: 'accounts@example.test', phoneNumber: '+359', contactPerson: 'Ada',
  paymentTerm: '2026-02-02T00:00:00.000Z', totalValueExcludingVat: 100, vatRate: 20, vat: 20,
  totalValueIncludingVat: 120, paymentDate: null, paymentTime: null, paymentMethod: 'Bank', siteAllocations: []
};

describe('InvoiceDialogComponent', () => {
  const invoicesService = { updateInvoiceMutation: { isPending: () => false }, updateInvoice: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoiceDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: invoice },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: DashboardInvoicesService, useValue: invoicesService },
        { provide: DashboardPersonsService, useValue: { searchSuppliers: vi.fn(), getPersonById: vi.fn() } },
        { provide: DashboardSitesService, useValue: { searchSites: vi.fn() } }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('submits the edited invoice and closes on success', async () => {
    invoicesService.updateInvoice.mockResolvedValue(undefined);
    const fixture = TestBed.createComponent(InvoiceDialogComponent);

    await fixture.componentInstance.saveInvoice();

    expect(invoicesService.updateInvoice).toHaveBeenCalledWith(expect.objectContaining({ invoiceId: 'invoice-1', invoiceNumber: 'INV-42', totalValue: 100, vatRate: 20 }));
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('does not submit an invoice without supplier details', async () => {
    const fixture = TestBed.createComponent(InvoiceDialogComponent);
    fixture.componentInstance.supplierDetailsReady.set(false);

    await fixture.componentInstance.saveInvoice();

    expect(invoicesService.updateInvoice).not.toHaveBeenCalled();
    expect(fixture.componentInstance.supplierSearchControl.touched).toBe(true);
  });

  it('keeps the dialog open and exposes save failures', async () => {
    invoicesService.updateInvoice.mockRejectedValue(new Error('network unavailable'));
    const fixture = TestBed.createComponent(InvoiceDialogComponent);

    await fixture.componentInstance.saveInvoice();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(fixture.componentInstance.saveError()).toBe('Unable to save the invoice.');
  });
});
