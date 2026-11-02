import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { ProductCatalogLookupService } from '../../../../shared/product-catalog/product-catalog-lookup.service';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { ProductRequirementDialogComponent } from './product-requirement-dialog.component';

describe('ProductRequirementDialogComponent', () => {
  const data = { activityId: 'activity-1', sectionId: 'section-1' };
  const catalogService = {
    mutation: { isPending: () => false },
    createProductRequirement: vi.fn(), updateProductRequirement: vi.fn()
  };
  const catalogLookupService = { getProductTypes: vi.fn(), getCategories: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductRequirementDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ActivityCatalogService, useValue: catalogService },
        { provide: ProductCatalogLookupService, useValue: catalogLookupService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('requires a Product Type, enforces four decimal places, and submits normalized notes', async () => {
    const component = TestBed.createComponent(ProductRequirementDialogComponent).componentInstance;
    component.form.patchValue({ productTypeId: '', quantity: 1.00001 });
    await component.submit();
    expect(catalogService.createProductRequirement).not.toHaveBeenCalled();

    catalogService.createProductRequirement.mockResolvedValue({ id: 'requirement-1' });
    component.productTypeControl.setValue({ id: 'product-type-1', name: 'Anchor', categoryId: 'category-1', categoryName: 'Fixings' });
    component.form.patchValue({ quantity: 2.5, isRequired: false, quantityBehavior: 'fixed', notes: '  Use anchors  ' });
    await component.submit();

    expect(catalogService.createProductRequirement).toHaveBeenCalledWith('activity-1', 'section-1', {
      productTypeId: 'product-type-1', quantity: 2.5, isRequired: false, quantityBehavior: 'fixed', notes: 'Use anchors'
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('keeps the dialog open and exposes save failures', async () => {
    const component = TestBed.createComponent(ProductRequirementDialogComponent).componentInstance;
    catalogService.createProductRequirement.mockRejectedValue(new Error('offline'));
    component.productTypeControl.setValue({ id: 'product-type-1', name: 'Anchor', categoryId: 'category-1', categoryName: 'Fixings' });
    component.form.patchValue({ quantity: 1 });

    await component.submit();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.errorMessage()).toContain('could not be updated');
  });
});
