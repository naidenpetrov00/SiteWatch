import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';

import { OfferActivity } from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';
import {
  fourDecimalPlaces,
  formatOfferQuantity,
  measurementUnitLabel
} from '../../utils/offer-quantity';

type ActivityMeasurementsForm = FormGroup<{
  measurements: FormArray<FormControl<number>>;
}>;

interface RemoveActivityDialogData {
  numberId: number;
  name: string;
}

@Component({
  selector: 'app-remove-offer-activity-dialog',
  imports: [MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>Remove Activity?</h2>
    <mat-dialog-content>
      <p>
        Remove #{{ data.numberId }} · {{ data.name }} and all of its calculated
        contributions from this Offer?
      </p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [mat-dialog-close]="false">Cancel</button>
      <button mat-flat-button color="warn" type="button" [mat-dialog-close]="true">
        Remove Activity
      </button>
    </mat-dialog-actions>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
class RemoveOfferActivityDialogComponent {
  readonly data = inject<RemoveActivityDialogData>(MAT_DIALOG_DATA);
}

@Component({
  selector: 'app-offer-selected-activities',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule
  ],
  templateUrl: './offer-selected-activities.component.html',
  styleUrl: './offer-selected-activities.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferSelectedActivitiesComponent {
  readonly offersService = inject(OffersService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);

  readonly siteId = input.required<string>();
  readonly offerId = input.required<string>();
  readonly activities = input.required<readonly OfferActivity[]>();
  readonly editable = input(false);
  readonly forms = signal<ReadonlyMap<string, ActivityMeasurementsForm>>(new Map());
  readonly feedbackMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly isMutating = computed(
    () =>
      this.offersService.updateMeasurementsMutation.isPending() ||
      this.offersService.removeActivityMutation.isPending()
  );

  measurementUnit = measurementUnitLabel;
  formatQuantity = formatOfferQuantity;

  constructor() {
    effect(() => {
      const editable = this.editable();
      const forms = new Map<string, ActivityMeasurementsForm>();
      for (const activity of this.activities()) {
        const controls = activity.sections.map((section) =>
          this.formBuilder.nonNullable.control(
            { value: section.requestedMeasurement, disabled: !editable },
            [
              Validators.required,
              Validators.min(Number.MIN_VALUE),
              fourDecimalPlaces()
            ]
          )
        );
        forms.set(
          activity.id,
          this.formBuilder.group({
            measurements: this.formBuilder.array(controls)
          })
        );
      }
      this.forms.set(forms);
    });
  }

  formFor(activityId: string): ActivityMeasurementsForm {
    const form = this.forms().get(activityId);
    if (!form) {
      throw new Error(`Missing measurement form for Offer activity ${activityId}.`);
    }
    return form;
  }

  async save(activity: OfferActivity): Promise<void> {
    const form = this.formFor(activity.id);
    if (!this.editable() || form.invalid || this.isMutating()) {
      form.markAllAsTouched();
      return;
    }

    this.clearFeedback();
    try {
      await this.offersService.updateActivityMeasurements({
        siteId: this.siteId(),
        offerId: this.offerId(),
        offerActivityId: activity.id,
        sectionMeasurements: activity.sections.map((section, index) => ({
          sectionId: section.id,
          requestedMeasurement:
            form.controls.measurements.controls[index].getRawValue()
        }))
      });
      this.feedbackMessage.set(`Measurements saved for ${activity.name}.`);
    } catch (error) {
      this.errorMessage.set(getOfferError(error));
    }
  }

  async confirmRemove(activity: OfferActivity): Promise<void> {
    if (!this.editable() || this.isMutating()) return;

    const dialogRef = this.dialog.open<
      RemoveOfferActivityDialogComponent,
      RemoveActivityDialogData,
      boolean
    >(RemoveOfferActivityDialogComponent, {
      autoFocus: false,
      width: '34rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        numberId: activity.activityNumberId,
        name: activity.name
      }
    });
    if (!(await firstValueFrom(dialogRef.afterClosed()))) return;

    this.clearFeedback();
    try {
      await this.offersService.removeActivity({
        siteId: this.siteId(),
        offerId: this.offerId(),
        offerActivityId: activity.id
      });
      this.feedbackMessage.set(`${activity.name} was removed from the Offer.`);
    } catch (error) {
      this.errorMessage.set(getOfferError(error));
    }
  }

  private clearFeedback(): void {
    this.feedbackMessage.set(null);
    this.errorMessage.set(null);
  }
}
