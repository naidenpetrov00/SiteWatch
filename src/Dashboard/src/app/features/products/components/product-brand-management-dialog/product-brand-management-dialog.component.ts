import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import { ProductCatalogBrand } from '../../../../shared/product-catalog/product-catalog.models';
import { ProductCatalogLookupService } from '../../../../shared/product-catalog/product-catalog-lookup.service';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';
import { CatalogConfirmDialogComponent, CatalogConfirmDialogData } from '../catalog-confirm-dialog/catalog-confirm-dialog.component';
import { ProductBrandEditorDialogComponent, ProductBrandEditorDialogData } from '../product-brand-editor-dialog/product-brand-editor-dialog.component';

@Component({
  selector: 'app-product-brand-management-dialog',
  imports: [ReactiveFormsModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatPaginatorModule],
  templateUrl: './product-brand-management-dialog.component.html',
  styleUrl: './product-brand-management-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductBrandManagementDialogComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly catalogService = inject(ProductCatalogService);
  private readonly catalogLookupService = inject(ProductCatalogLookupService);

  readonly searchControl = this.formBuilder.nonNullable.control('');
  readonly searchTerm = signal('');
  readonly pageIndex = signal(0);
  readonly pageSize = signal(25);
  readonly pageError = signal<string | null>(null);
  readonly deletingId = signal<string | null>(null);
  readonly queryState = computed(() => ({
    searchTerm: this.searchTerm(),
    pageIndex: this.pageIndex(),
    pageSize: this.pageSize()
  }));
  readonly productBrandsQuery = injectQuery(() => ({
    queryKey: ['product-catalog', 'product-brands', this.queryState()] as const,
    queryFn: () => this.catalogLookupService.getProductBrands(this.queryState())
  }));

  constructor() {
    this.searchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((searchTerm) => {
        this.searchTerm.set(searchTerm.trim());
        this.pageIndex.set(0);
      });
  }

  async createProductBrand(): Promise<void> { await this.openEditor(null); }
  async editProductBrand(brand: ProductCatalogBrand): Promise<void> { await this.openEditor(brand); }

  async deleteProductBrand(brand: ProductCatalogBrand): Promise<void> {
    if (this.deletingId()) return;
    const confirmRef = this.dialog.open<CatalogConfirmDialogComponent, CatalogConfirmDialogData, boolean>(CatalogConfirmDialogComponent, {
      autoFocus: 'button', restoreFocus: true, width: '32rem', maxWidth: 'calc(100vw - 2rem)',
      data: {
        title: 'Delete Product Brand',
        message: `Delete ${brand.name}? This is allowed only when no Products use it.`,
        confirmLabel: 'Delete Product Brand'
      }
    });
    if (!(await firstValueFrom(confirmRef.afterClosed()))) return;

    this.deletingId.set(brand.id);
    this.pageError.set(null);
    try {
      await this.catalogService.deleteProductBrand(brand.id);
      await this.productBrandsQuery.refetch();
    } catch (error) {
      this.pageError.set(getProductCatalogError(error, 'The Product Brand could not be deleted.'));
      if ((error as { status?: number }).status === 404) await this.productBrandsQuery.refetch();
    } finally {
      this.deletingId.set(null);
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
  }

  async retry(): Promise<void> {
    this.pageError.set(null);
    await this.productBrandsQuery.refetch();
  }

  private async openEditor(productBrand: ProductCatalogBrand | null): Promise<void> {
    this.pageError.set(null);
    const dialogRef = this.dialog.open<ProductBrandEditorDialogComponent, ProductBrandEditorDialogData, ProductCatalogBrand | null>(ProductBrandEditorDialogComponent, {
      autoFocus: 'input[formControlName="name"]', restoreFocus: true, width: '30rem', maxWidth: 'calc(100vw - 2rem)', data: { productBrand }
    });
    if (await firstValueFrom(dialogRef.afterClosed())) await this.productBrandsQuery.refetch();
  }
}
