import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { ProductCatalogLookupService } from '../../../../shared/product-catalog/product-catalog-lookup.service';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { ProductBrandEditorDialogComponent } from './product-brand-editor-dialog.component';

describe('ProductBrandEditorDialogComponent', () => {
  const catalogLookupService = { getProductBrands: vi.fn() };
  const catalogService = { createProductBrand: vi.fn(), updateProductBrand: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductBrandEditorDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: { productBrand: null } },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ProductCatalogLookupService, useValue: catalogLookupService },
        { provide: ProductCatalogService, useValue: catalogService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  afterEach(() => vi.useRealTimers());

  it('suggests matching Brands and blocks an exact normalized duplicate', async () => {
    vi.useFakeTimers();
    catalogLookupService.getProductBrands.mockResolvedValue({
      items: [{ id: 'brand-1', name: 'Bosch Professional' }],
      filteredCount: 1,
      totalCount: 1
    });
    const component = TestBed.createComponent(ProductBrandEditorDialogComponent).componentInstance;

    component.form.controls.name.setValue('  bosch   professional ');
    await vi.advanceTimersByTimeAsync(250);

    expect(catalogLookupService.getProductBrands).toHaveBeenCalledWith({
      searchTerm: 'bosch   professional',
      pageSize: 5
    });
    expect(component.exactDuplicate()).toBe(true);
    await component.save();
    expect(catalogService.createProductBrand).not.toHaveBeenCalled();
  });
});
