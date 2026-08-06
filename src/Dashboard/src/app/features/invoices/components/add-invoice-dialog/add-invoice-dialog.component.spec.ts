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
});
