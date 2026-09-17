import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardPersonsService } from '../../../persons/services/dashboard-persons.service';
import { DashboardRetailersService } from '../../services/dashboard-retailers.service';
import { RetailerDialogComponent } from './retailer-dialog.component';

describe('RetailerDialogComponent', () => {
  const dialogRef = { close: vi.fn() };
  const persons = { searchCompanies: vi.fn() };
  const retailers = {
    createRetailerMutation: { isPending: () => false },
    updateRetailerMutation: { isPending: () => false },
    createRetailer: vi.fn(),
    updateRetailer: vi.fn()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RetailerDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: null },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: DashboardPersonsService, useValue: persons },
        { provide: DashboardRetailersService, useValue: retailers }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('does not submit an incomplete retailer form', async () => {
    const component = TestBed.createComponent(RetailerDialogComponent).componentInstance;

    await component.submitRetailer();

    expect(retailers.createRetailer).not.toHaveBeenCalled();
  });

  it('uses a selected company and submits normalized create details', async () => {
    retailers.createRetailer.mockResolvedValue({ id: 'retailer-1' });
    const component = TestBed.createComponent(RetailerDialogComponent).componentInstance;
    component.retailerForm.patchValue({ displayName: '  Example   Store ', baseWebsiteUrl: ' https://example.com ', notes: '  Note  ' });
    component.onCompanySelected({ option: { value: { id: 'company-1', displayName: 'Example Ltd', legalForm: null, eik: '123456789', vatNumber: 'BG123' } } } as never);

    await component.submitRetailer();

    expect(retailers.createRetailer).toHaveBeenCalledWith({ displayName: 'Example Store', companyPersonId: 'company-1', baseWebsiteUrl: 'https://example.com', notes: 'Note' });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('keeps the dialog open and exposes the API validation message when saving fails', async () => {
    retailers.createRetailer.mockRejectedValue(new HttpErrorResponse({ status: 400, error: { details: [{ message: 'CompanyPersonId must reference an existing company Person.' }] } }));
    const component = TestBed.createComponent(RetailerDialogComponent).componentInstance;
    component.retailerForm.patchValue({ displayName: 'Example Store', companyPersonId: 'company-1', baseWebsiteUrl: 'https://example.com' });
    component.companySearchControl.setValue({ id: 'company-1', displayName: 'Example Ltd', legalForm: null, eik: '123456789', vatNumber: 'BG123' });

    await component.submitRetailer();

    expect(component.saveError()).toBe('CompanyPersonId must reference an existing company Person.');
    expect(dialogRef.close).not.toHaveBeenCalled();
  });
});
