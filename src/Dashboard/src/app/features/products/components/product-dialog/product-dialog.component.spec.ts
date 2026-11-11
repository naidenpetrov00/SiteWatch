import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardProductsService } from '../../services/dashboard-products.service';
import { ProductCatalogLookupService } from '../../../../shared/product-catalog/product-catalog-lookup.service';
import { ProductDialogComponent } from './product-dialog.component';

describe('ProductDialogComponent', () => {
  const productsService = {
    createProductMutation: { isPending: () => false }, updateProductMutation: { isPending: () => false },
    createProduct: vi.fn(), updateProduct: vi.fn()
  };
  const dialogRef = { close: vi.fn() };
  const catalogLookupService = { getProductTypes: vi.fn(), getCategories: vi.fn(), getProductBrands: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: null },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: DashboardProductsService, useValue: productsService },
        { provide: ProductCatalogLookupService, useValue: catalogLookupService }
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
    component.productTypeControl.setValue({ id: 'product-type-1', name: 'Tools Equipment', categoryId: 'category-1', categoryName: 'Tools' });
    component.productBrandControl.setValue({ id: 'brand-1', name: 'Bosch' });
    component.productForm.patchValue({
      title: '  Cordless   Drill ', model: ' GSB ',
      status: 'Active', packageQuantity: 1, packageUnit: 'piece', primarySearchPhrase: ' '
    });
    component.addSearchEntry('alternativeSearchPhrases');
    component.addSearchEntry('alternativeSearchPhrases');
    component.productForm.controls.alternativeSearchPhrases.at(0).setValue(' Drill Driver ');
    component.productForm.controls.alternativeSearchPhrases.at(1).setValue('drill driver');

    await component.submitProduct();

    expect(productsService.createProduct).toHaveBeenCalledWith(expect.objectContaining({
      title: 'Cordless   Drill', brandId: 'brand-1', model: 'GSB', primarySearchPhrase: null,
      alternativeSearchPhrases: ['Drill Driver']
    }));
    expect(dialogRef.close).toHaveBeenCalledWith({ createdProductId: 'product-1' });
  });

  it('keeps the dialog open after a save failure', async () => {
    productsService.createProduct.mockRejectedValue(new Error('offline'));
    const component = TestBed.createComponent(ProductDialogComponent).componentInstance;
    component.productTypeControl.setValue({ id: 'product-type-1', name: 'Tools Equipment', categoryId: 'category-1', categoryName: 'Tools' });
    component.productForm.patchValue({ title: 'Drill', status: 'Active' });

    await component.submitProduct();

    expect(productsService.createProduct).toHaveBeenCalled();
    expect(dialogRef.close).not.toHaveBeenCalled();
  });
});
