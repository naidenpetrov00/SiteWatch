import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { ProductCatalogCategory } from '../../models/product-catalog.models';
import { ProductCatalogService } from '../../services/product-catalog.service';
import { getProductCatalogError } from '../../utils/product-catalog-error';
import { trimmedRequiredValidator } from '../../utils/product-catalog.validators';

export interface CategoryEditorDialogData {
  category: ProductCatalogCategory | null;
}

@Component({
  selector: 'app-category-editor-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './category-editor-dialog.component.html',
  styleUrl: './category-editor-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryEditorDialogComponent {
  readonly data = inject<CategoryEditorDialogData>(MAT_DIALOG_DATA);
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogService = inject(ProductCatalogService);
  private readonly dialogRef = inject(
    MatDialogRef<CategoryEditorDialogComponent, ProductCatalogCategory | null>
  );

  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly isEdit = this.data.category !== null;
  readonly formId = this.isEdit ? 'edit-category-form' : 'create-category-form';
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control(this.data.category?.name ?? '', [
      trimmedRequiredValidator(),
      Validators.maxLength(200)
    ])
  });

  close(): void {
    if (!this.saving()) this.dialogRef.close(null);
  }

  async save(): Promise<void> {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);
    try {
      const request = { name: this.form.controls.name.value.trim() };
      const category = this.data.category
        ? await this.catalogService.updateCategory(this.data.category.id, request)
        : await this.catalogService.createCategory(request);
      this.dialogRef.close(category);
    } catch (error) {
      this.saveError.set(getProductCatalogError(error, 'The Category could not be saved.'));
    } finally {
      this.saving.set(false);
    }
  }
}
