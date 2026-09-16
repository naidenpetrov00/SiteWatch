import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardProductsService } from '../../../products/services/dashboard-products.service';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { ProductRequirementDialogComponent } from './product-requirement-dialog.component';

describe('ProductRequirementDialogComponent', () => {
  const data = { activityId: 'activity-1', sectionId: 'section-1', excludedProductIds: [] };
  const catalogService = {
    mutation: { isPending: () => false },
    createProductRequirement: vi.fn(), updateProductRequirement: vi.fn()
  };
  const productsService = { searchProducts: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductRequirementDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ActivityCatalogService, useValue: catalogService },
        { provide: DashboardProductsService, useValue: productsService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('requires a product, enforces four decimal places, and submits normalized notes', async () => {
    const component = TestBed.createComponent(ProductRequirementDialogComponent).componentInstance;
    component.form.patchValue({ productId: '', quantity: 1.00001 });
    await component.submit();
    expect(catalogService.createProductRequirement).not.toHaveBeenCalled();

    catalogService.createProductRequirement.mockResolvedValue({ id: 'requirement-1' });
    component.form.patchValue({ productId: 'product-1', quantity: 2.5, isRequired: false, quantityBehavior: 'fixed', notes: '  Use anchors  ' });
    await component.submit();

    expect(catalogService.createProductRequirement).toHaveBeenCalledWith('activity-1', 'section-1', {
      productId: 'product-1', quantity: 2.5, isRequired: false, quantityBehavior: 'fixed', notes: 'Use anchors'
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('keeps the dialog open and exposes save failures', async () => {
    const component = TestBed.createComponent(ProductRequirementDialogComponent).componentInstance;
    catalogService.createProductRequirement.mockRejectedValue(new Error('offline'));
    component.form.patchValue({ productId: 'product-1', quantity: 1 });

    await component.submit();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.errorMessage()).toContain('could not be updated');
  });
});
