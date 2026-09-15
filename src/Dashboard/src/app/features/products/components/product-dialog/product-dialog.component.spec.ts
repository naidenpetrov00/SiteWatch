import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardProductsService } from '../../services/dashboard-products.service';
import { ProductDialogComponent } from './product-dialog.component';

describe('ProductDialogComponent', () => {
  const productsService = {
    createProductMutation: { isPending: () => false }, updateProductMutation: { isPending: () => false },
    createProduct: vi.fn(), updateProduct: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: null },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: DashboardProductsService, useValue: productsService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('enforces the package pair and collection limit', () => {
    const component = TestBed.createComponent(ProductDialogComponent).componentInstance;
    component.productForm.patchValue({ packageQuantity: 1, packageUnit: null });

    expect(component.productForm.hasError('packagePair')).toBe(true);

    for (let index = 0; index < 21; index += 1) {
      component.addSearchEntry('alternativeSearchPhrases');
    }
    expect(component.productForm.controls.alternativeSearchPhrases.length).toBe(20);
  });

  it('normalizes a valid create request and closes after success', async () => {
    productsService.createProduct.mockResolvedValue({ id: 'product-1' });
    const component = TestBed.createComponent(ProductDialogComponent).componentInstance;
    component.productForm.patchValue({
      title: '  Cordless   Drill ', brand: ' Bosch ', model: ' GSB ', category: 'tools-equipment',
      status: 'Active', packageQuantity: 1, packageUnit: 'piece', primarySearchPhrase: ' '
    });
    component.addSearchEntry('alternativeSearchPhrases');
    component.addSearchEntry('alternativeSearchPhrases');
    component.productForm.controls.alternativeSearchPhrases.at(0).setValue(' Drill Driver ');
    component.productForm.controls.alternativeSearchPhrases.at(1).setValue('drill driver');

    await component.submitProduct();

    expect(productsService.createProduct).toHaveBeenCalledWith(expect.objectContaining({
      title: 'Cordless   Drill', brand: 'Bosch', model: 'GSB', primarySearchPhrase: null,
      alternativeSearchPhrases: ['Drill Driver']
    }));
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('keeps the dialog open after a save failure', async () => {
    productsService.createProduct.mockRejectedValue(new Error('offline'));
    const component = TestBed.createComponent(ProductDialogComponent).componentInstance;
    component.productForm.patchValue({ title: 'Drill', category: 'tools-equipment', status: 'Active' });

    await component.submitProduct();

    expect(productsService.createProduct).toHaveBeenCalled();
    expect(dialogRef.close).not.toHaveBeenCalled();
  });
});
