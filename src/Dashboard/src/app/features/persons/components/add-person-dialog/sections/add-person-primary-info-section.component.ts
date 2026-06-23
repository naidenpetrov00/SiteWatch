import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { startWith, switchMap } from 'rxjs';

import {
  AddPersonDialogFormGroup,
  LEGAL_FORM_OPTIONS,
  PERSON_TYPE_OPTIONS,
  PERSON_TYPES,
  VAT_COUNTRY_CODE_OPTIONS
} from '../add-person-dialog.types';
import { ADD_PERSON_VALIDATION_LIMITS } from '../add-person-dialog.validators';
import { PersonInputSanitizerDirective } from '../directives/person-input-sanitizer.directive';

@Component({
  selector: 'app-add-person-primary-info-section',
  imports: [
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PersonInputSanitizerDirective
  ],
  templateUrl: './add-person-primary-info-section.component.html',
  styleUrl: './add-person-primary-info-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonPrimaryInfoSectionComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly personForm = input.required<AddPersonDialogFormGroup>();
  readonly syncVatNumber = input(true);
  readonly personTypeOptions = PERSON_TYPE_OPTIONS;
  readonly legalFormOptions = LEGAL_FORM_OPTIONS;
  readonly vatCountryCodeOptions = VAT_COUNTRY_CODE_OPTIONS;
  readonly personTypes = PERSON_TYPES;
  readonly validationLimits = ADD_PERSON_VALIDATION_LIMITS;

  ngOnInit(): void {
    if (!this.syncVatNumber()) {
      return;
    }

    const personForm = this.personForm();
    const typeControl = personForm.controls.type;
    const eikControl = personForm.controls.eik;
    const egnControl = personForm.controls.egn;
    const vatNumberControl = personForm.controls.vatNumber;

    typeControl.valueChanges
      .pipe(
        startWith(typeControl.value),
        switchMap((type) => {
          const sourceControl = type === PERSON_TYPES.company ? eikControl : egnControl;

          return sourceControl.valueChanges.pipe(startWith(sourceControl.value));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((sourceValue) => {
        if (vatNumberControl.value === sourceValue) {
          return;
        }

        vatNumberControl.setValue(sourceValue, { emitEvent: false });
      });
  }
}
