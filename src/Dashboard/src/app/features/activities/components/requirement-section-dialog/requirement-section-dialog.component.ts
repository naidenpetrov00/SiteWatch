import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  ACTIVITY_MEASUREMENT_UNIT_OPTIONS,
  ActivityMeasurementUnit,
  ActivityRequirementSection
} from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { getActivityCatalogError } from '../../utils/activity-catalog-error';

export interface RequirementSectionDialogData {
  activityId: string;
  section?: ActivityRequirementSection;
}

@Component({
  selector: 'app-requirement-section-dialog',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './requirement-section-dialog.component.html',
  styleUrl: './requirement-section-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RequirementSectionDialogComponent {
  readonly data = inject<RequirementSectionDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(
    MatDialogRef<RequirementSectionDialogComponent>
  );
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogService = inject(ActivityCatalogService);

  readonly units = ACTIVITY_MEASUREMENT_UNIT_OPTIONS;
  readonly errorMessage = signal<string | null>(null);
  readonly formId = this.data.section
    ? 'edit-requirement-section-form'
    : 'add-requirement-section-form';
  readonly title = this.data.section ? 'Edit Section' : 'Add Section';
  readonly subtitle = this.data.section
    ? 'Update the measurement basis for this group of products.'
    : 'Define the measurement basis before assigning products.';
  readonly submitLabel = this.data.section ? 'Save Section' : 'Add Section';
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control(this.data.section?.name ?? '', [
      Validators.maxLength(200)
    ]),
    basisQuantity: this.formBuilder.nonNullable.control(
      this.data.section?.basisQuantity ?? 1,
      [Validators.required, Validators.min(Number.MIN_VALUE), fourDecimalPlaces()]
    ),
    measurementUnit: this.formBuilder.nonNullable.control<ActivityMeasurementUnit>(
      this.data.section?.measurementUnit ?? 'piece',
      [Validators.required]
    )
  });

  readonly isSaving = (): boolean => this.catalogService.mutation.isPending();

  close(): void {
    this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    const request = {
      name: value.name.trim().replace(/\s+/g, ' ') || null,
      basisQuantity: value.basisQuantity,
      measurementUnit: value.measurementUnit
    };

    try {
      if (this.data.section) {
        await this.catalogService.updateRequirementSection(
          this.data.activityId,
          this.data.section.id,
          request
        );
      } else {
        await this.catalogService.createRequirementSection(
          this.data.activityId,
          request
        );
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
    return Math.abs(scaled - Math.round(scaled)) < 0.000001
      ? null
      : { decimalScale: true };
  };
}
