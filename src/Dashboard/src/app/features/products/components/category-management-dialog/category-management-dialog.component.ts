import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import { ProductCatalogCategory } from '../../models/product-catalog.models';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';
import {
  CatalogConfirmDialogComponent,
  CatalogConfirmDialogData
} from '../catalog-confirm-dialog/catalog-confirm-dialog.component';
import {
  CategoryEditorDialogComponent,
  CategoryEditorDialogData
} from '../category-editor-dialog/category-editor-dialog.component';

@Component({
  selector: 'app-category-management-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule
  ],
  templateUrl: './category-management-dialog.component.html',
  styleUrl: './category-management-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryManagementDialogComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly catalogService = inject(ProductCatalogService);

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
  readonly categoriesQuery = injectQuery(() => ({
    queryKey: ['product-catalog', 'categories', this.queryState()] as const,
    queryFn: () => this.catalogService.getCategories(this.queryState())
  }));

  constructor() {
    this.searchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((searchTerm) => {
        this.searchTerm.set(searchTerm.trim());
        this.pageIndex.set(0);
      });
  }

  async createCategory(): Promise<void> {
    await this.openEditor(null);
  }

  async editCategory(category: ProductCatalogCategory): Promise<void> {
    await this.openEditor(category);
  }

  async deleteCategory(category: ProductCatalogCategory): Promise<void> {
    if (this.deletingId()) return;
    const confirmRef = this.dialog.open<
      CatalogConfirmDialogComponent,
      CatalogConfirmDialogData,
      boolean
    >(CatalogConfirmDialogComponent, {
      autoFocus: 'button',
      restoreFocus: true,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        title: 'Delete Category',
        message: `Delete ${category.name}? This is allowed only when no Product Types use it.`,
        confirmLabel: 'Delete Category'
      }
    });
    if (!(await firstValueFrom(confirmRef.afterClosed()))) return;

    this.deletingId.set(category.id);
    this.pageError.set(null);
    try {
      await this.catalogService.deleteCategory(category.id);
      await this.categoriesQuery.refetch();
    } catch (error) {
      this.pageError.set(getProductCatalogError(error, 'The Category could not be deleted.'));
      if ((error as { status?: number }).status === 404) await this.categoriesQuery.refetch();
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
    await this.categoriesQuery.refetch();
  }

  private async openEditor(category: ProductCatalogCategory | null): Promise<void> {
    this.pageError.set(null);
    const dialogRef = this.dialog.open<
      CategoryEditorDialogComponent,
      CategoryEditorDialogData,
      ProductCatalogCategory | null
    >(CategoryEditorDialogComponent, {
      autoFocus: 'input[formControlName="name"]',
      restoreFocus: true,
      width: '30rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { category }
    });
    if (await firstValueFrom(dialogRef.afterClosed())) await this.categoriesQuery.refetch();
  }
}
