import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { CatalogSelectorComponent } from '../../../../shared/product-catalog/catalog-selector/catalog-selector.component';
import { ProductCatalogProductType } from '../../../../shared/product-catalog/product-catalog.models';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { ActivityProductRequirement, ProductQuantityBehavior } from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { getActivityCatalogError } from '../../utils/activity-catalog-error';

export interface ProductRequirementDialogData {
  activityId: string;
  sectionId: string;
  requirement?: ActivityProductRequirement;
}

@Component({
  selector: 'app-product-requirement-dialog',
  imports: [ReactiveFormsModule, CatalogSelectorComponent, MatFormFieldModule, MatInputModule, MatSelectModule, DialogActionBarComponent, DialogShellComponent],
  templateUrl: './product-requirement-dialog.component.html',
  styleUrl: './product-requirement-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductRequirementDialogComponent {
  readonly data = inject<ProductRequirementDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ProductRequirementDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogService = inject(ActivityCatalogService);

  readonly selectedProductType = signal<ProductCatalogProductType | null>(this.data.requirement ? {
    id: this.data.requirement.productTypeId,
    name: this.data.requirement.productTypeName,
    description: this.data.requirement.productTypeDescription,
    categoryId: this.data.requirement.categoryId,
    categoryName: this.data.requirement.categoryName
  } : null);
  readonly productTypeControl = this.formBuilder.control<ProductCatalogProductType | null>(this.selectedProductType(), [Validators.required]);
  readonly errorMessage = signal<string | null>(null);
  readonly formId = this.data.requirement ? 'edit-product-requirement-form' : 'add-product-requirement-form';
  readonly title = this.data.requirement ? 'Edit Product Type Requirement' : 'Add Product Type Requirement';
  readonly subtitle = this.data.requirement
    ? 'Update the Product Type and how it is used by this measurement section.'
    : 'Choose a Product Type and configure the required quantity.';
  readonly submitLabel = this.data.requirement ? 'Save Requirement' : 'Add Requirement';
  readonly form = this.formBuilder.group({
    productTypeId: this.formBuilder.nonNullable.control(this.data.requirement?.productTypeId ?? '', [Validators.required]),
    quantity: this.formBuilder.nonNullable.control(this.data.requirement?.quantity ?? 1, [Validators.required, Validators.min(Number.MIN_VALUE), fourDecimalPlaces()]),
    isRequired: this.formBuilder.nonNullable.control(this.data.requirement?.isRequired ?? true),
    quantityBehavior: this.formBuilder.nonNullable.control<ProductQuantityBehavior>(this.data.requirement?.quantityBehavior ?? 'proportional', [Validators.required]),
    notes: this.formBuilder.nonNullable.control(this.data.requirement?.notes ?? '', [Validators.maxLength(1000)])
  });

  readonly isSaving = (): boolean => this.catalogService.mutation.isPending();

  constructor() {
    this.productTypeControl.valueChanges.subscribe((productType) => {
      this.selectedProductType.set(productType);
      this.form.controls.productTypeId.setValue(productType?.id ?? '');
    });
  }

  close(): void {
    this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.productTypeControl.markAsTouched();
      return;
    }

    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    const request = {
      productTypeId: value.productTypeId,
      quantity: value.quantity,
      isRequired: value.isRequired,
      quantityBehavior: value.quantityBehavior,
      notes: value.notes.trim() || null
    };

    try {
      if (this.data.requirement) {
        await this.catalogService.updateProductRequirement(this.data.activityId, this.data.sectionId, this.data.requirement.id, request);
      } else {
        await this.catalogService.createProductRequirement(this.data.activityId, this.data.sectionId, request);
      }
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(getActivityCatalogError(error));
    }
  }
}

function fourDecimalPlaces(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = Number(control.value);
    if (!Number.isFinite(value)) return { decimalScale: true };
    const scaled = value * 10_000;
    return Math.abs(scaled - Math.round(scaled)) < 0.000001 ? null : { decimalScale: true };
  };
}
