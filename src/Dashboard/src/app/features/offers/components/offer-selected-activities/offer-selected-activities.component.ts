import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked
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
  private configuredOfferIdentity: string | null = null;
  private sectionIdsByActivity = new Map<string, readonly string[]>();

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
      const offerIdentity = `${this.siteId()}/${this.offerId()}`;
      const editable = this.editable();
      if (this.configuredOfferIdentity !== offerIdentity) {
        this.forms.set(new Map());
        this.sectionIdsByActivity = new Map();
        this.configuredOfferIdentity = offerIdentity;
      }

      this.reconcileForms(
        this.activities(),
        editable,
        untracked(() => this.forms())
      );
    });
  }

  hasUnsavedEdits(): boolean {
    return [...this.forms().values()].some((form) => form.dirty);
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
      form.reset(
        {
          measurements: form.controls.measurements.controls.map((control) =>
            control.getRawValue()
          )
        },
        { emitEvent: false }
      );
      this.setFormEditable(form, this.editable());
      this.reconcileForms(
        this.activities(),
        this.editable(),
        untracked(() => this.forms())
      );
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

  private reconcileForms(
    activities: readonly OfferActivity[],
    editable: boolean,
    existingForms: ReadonlyMap<string, ActivityMeasurementsForm>
  ): void {
    const forms = new Map<string, ActivityMeasurementsForm>();
    const sectionIdsByActivity = new Map<string, readonly string[]>();

    for (const activity of activities) {
      const existingForm = existingForms.get(activity.id);
      const previousSectionIds = this.sectionIdsByActivity.get(activity.id) ?? [];
      const form = existingForm
        ? this.reconcileActivityForm(
            existingForm,
            previousSectionIds,
            activity,
            editable
          )
        : this.createActivityForm(activity, editable);

      forms.set(activity.id, form);
      sectionIdsByActivity.set(
        activity.id,
        activity.sections.map((section) => section.id)
      );
    }

    this.sectionIdsByActivity = sectionIdsByActivity;
    this.forms.set(forms);
  }

  private reconcileActivityForm(
    form: ActivityMeasurementsForm,
    previousSectionIds: readonly string[],
    activity: OfferActivity,
    editable: boolean
  ): ActivityMeasurementsForm {
    const controlsBySectionId = new Map<string, FormControl<number>>(
      previousSectionIds.map(
        (sectionId, index) =>
          [
            sectionId,
            form.controls.measurements.controls[index]
          ] as const
      )
    );
    const controls = activity.sections.map((section) => {
      const control = controlsBySectionId.get(section.id);
      if (!control) {
        return this.createMeasurementControl(section.requestedMeasurement, editable);
      }

      if (!editable || !control.dirty) {
        control.reset(
          { value: section.requestedMeasurement, disabled: !editable },
          { emitEvent: false }
        );
      } else if (editable) {
        control.enable({ emitEvent: false });
      } else {
        control.disable({ emitEvent: false });
      }
      return control;
    });

    if (!this.hasSameSectionOrder(previousSectionIds, activity)) {
      form.setControl('measurements', this.formBuilder.array(controls));
    }
    this.setFormEditable(form, editable);
    return form;
  }

  private createActivityForm(
    activity: OfferActivity,
    editable: boolean
  ): ActivityMeasurementsForm {
    return this.formBuilder.group({
      measurements: this.formBuilder.array(
        activity.sections.map((section) =>
          this.createMeasurementControl(section.requestedMeasurement, editable)
        )
      )
    });
  }

  private createMeasurementControl(
    value: number,
    editable: boolean
  ): FormControl<number> {
    return this.formBuilder.nonNullable.control(
      { value, disabled: !editable },
      [
        Validators.required,
        Validators.min(Number.MIN_VALUE),
        fourDecimalPlaces()
      ]
    );
  }

  private hasSameSectionOrder(
    previousSectionIds: readonly string[],
    activity: OfferActivity
  ): boolean {
    return (
      previousSectionIds.length === activity.sections.length &&
      previousSectionIds.every(
        (sectionId, index) => sectionId === activity.sections[index].id
      )
    );
  }

  private setFormEditable(form: ActivityMeasurementsForm, editable: boolean): void {
    if (editable) {
      form.enable({ emitEvent: false });
    } else {
      form.disable({ emitEvent: false });
    }
  }
}
