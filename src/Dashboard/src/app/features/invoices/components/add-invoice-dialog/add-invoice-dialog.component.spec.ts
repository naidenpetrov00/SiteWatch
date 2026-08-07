import { TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardPersonsService } from '../../../persons/services/dashboard-persons.service';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardInvoicesService } from '../../services/dashboard-invoices.service';
import { AddInvoiceDialogComponent } from './add-invoice-dialog.component';

describe('AddInvoiceDialogComponent', () => {
  const invoicesService = {
    createInvoiceMutation: { isPending: () => false },
    uploadInvoiceFileMutation: { isPending: () => false },
    createInvoice: vi.fn(),
    uploadInvoiceFile: vi.fn()
  };
  const personsService = { searchSuppliers: vi.fn(), getPersonById: vi.fn() };
  const sitesService = { searchSites: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddInvoiceDialogComponent],
      providers: [
        { provide: DashboardInvoicesService, useValue: invoicesService },
        { provide: DashboardPersonsService, useValue: personsService },
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('rejects an attachment whose type is not accepted by the invoice upload contract', () => {
    const fixture = TestBed.createComponent(AddInvoiceDialogComponent);
    const file = new File(['text'], 'invoice.txt', { type: 'text/plain' });
    const input = { files: [file], value: 'invoice.txt' } as unknown as HTMLInputElement;

    fixture.componentInstance.onAttachmentSelected({ target: input } as unknown as Event);

    expect(fixture.componentInstance.attachment()).toBeNull();
    expect(fixture.componentInstance.attachmentError()).toBe('Choose a PDF or supported image file.');
    expect(input.value).toBe('');
  });

  it('accepts a PDF attachment at the client validation boundary', () => {
    const fixture = TestBed.createComponent(AddInvoiceDialogComponent);
    const file = new File(['%PDF-'], 'invoice.pdf', { type: 'application/pdf' });
    const input = { files: [file], value: 'invoice.pdf' } as unknown as HTMLInputElement;

    fixture.componentInstance.onAttachmentSelected({ target: input } as unknown as Event);

    expect(fixture.componentInstance.attachment()).toBe(file);
    expect(fixture.componentInstance.attachmentError()).toBeNull();
  });

  it('rejects an attachment larger than the 20 MB upload limit', () => {
    const fixture = TestBed.createComponent(AddInvoiceDialogComponent);
    const file = new File(['pdf'], 'invoice.pdf', { type: 'application/pdf' });
    Object.defineProperty(file, 'size', { value: 20 * 1024 * 1024 + 1 });
    const input = { files: [file], value: 'invoice.pdf' } as unknown as HTMLInputElement;

    fixture.componentInstance.onAttachmentSelected({ target: input } as unknown as Event);

    expect(fixture.componentInstance.attachment()).toBeNull();
    expect(fixture.componentInstance.attachmentError()).toBe('The attachment cannot exceed 20 MB.');
  });

  it('creates the invoice before uploading its accepted attachment', async () => {
    invoicesService.createInvoice.mockResolvedValue({ id: 'invoice-1' });
    invoicesService.uploadInvoiceFile.mockResolvedValue(undefined);
    const fixture = TestBed.createComponent(AddInvoiceDialogComponent);
    const component = fixture.componentInstance;
    component.supplierDetailsReady.set(true);
    component.invoiceForm.patchValue({
      supplierId: '11111111-1111-1111-1111-111111111111',
      invoiceNumber: 'INV-42',
      date: new Date('2026-01-02T00:00:00.000Z'),
      paymentTerm: new Date('2026-02-02T00:00:00.000Z'),
      totalValue: 100,
      vatRate: 20,
      paymentMethod: 'Bank'
    });
    const file = new File(['%PDF-'], 'invoice.pdf', { type: 'application/pdf' });
    component.onAttachmentSelected({ target: { files: [file], value: 'invoice.pdf' } } as unknown as Event);

    await component.submitInvoice();

    expect(invoicesService.createInvoice).toHaveBeenCalledTimes(1);
    expect(invoicesService.uploadInvoiceFile).toHaveBeenCalledWith('invoice-1', file);
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });
});
