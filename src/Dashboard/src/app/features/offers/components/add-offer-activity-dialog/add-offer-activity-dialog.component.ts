import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { OfferActivityCandidate } from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';
import {
  fourDecimalPlaces,
  formatOfferQuantity,
  measurementUnitLabel
} from '../../utils/offer-quantity';

export interface AddOfferActivityDialogData {
  siteId: string;
  offerId: string;
  candidate: OfferActivityCandidate;
}

@Component({
  selector: 'app-add-offer-activity-dialog',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './add-offer-activity-dialog.component.html',
  styleUrl: './add-offer-activity-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddOfferActivityDialogComponent {
  readonly data = inject<AddOfferActivityDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(
    MatDialogRef<AddOfferActivityDialogComponent>
  );
  private readonly formBuilder = inject(FormBuilder);
  private readonly offersService = inject(OffersService);

  readonly formId = 'add-offer-activity-form';
  readonly errorMessage = signal<string | null>(null);
  readonly form = this.formBuilder.group({
    measurements: this.formBuilder.array(
      this.data.candidate.sections.map((section) =>
        this.formBuilder.nonNullable.control(section.basisQuantity, [
          Validators.required,
          Validators.min(Number.MIN_VALUE),
          fourDecimalPlaces()
        ])
      )
    )
  });

  isSaving(): boolean {
    return this.offersService.addActivityMutation.isPending();
  }

  measurementUnit = measurementUnitLabel;
  formatQuantity = formatOfferQuantity;

  close(): void {
    this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.form.invalid || this.isSaving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    try {
      await this.offersService.addActivity({
        siteId: this.data.siteId,
        offerId: this.data.offerId,
        activityId: this.data.candidate.id,
        sectionMeasurements: this.data.candidate.sections.map((section, index) => ({
          sectionId: section.id,
          requestedMeasurement:
            this.form.controls.measurements.controls[index].getRawValue()
        }))
      });
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(getOfferError(error));
    }
  }
}
