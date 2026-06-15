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

import {
  AddPersonDialogFormGroup,
  PERSON_TYPE_OPTIONS,
  VAT_COUNTRY_CODE_OPTIONS
} from '../add-person-dialog.types';

@Component({
  selector: 'app-add-person-primary-info-section',
  imports: [
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './add-person-primary-info-section.component.html',
  styleUrl: './add-person-primary-info-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonPrimaryInfoSectionComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly personForm = input.required<AddPersonDialogFormGroup>();
  readonly personTypeOptions = PERSON_TYPE_OPTIONS;
  readonly vatCountryCodeOptions = VAT_COUNTRY_CODE_OPTIONS;

  ngOnInit(): void {
    const personForm = this.personForm();
    const eikControl = personForm.controls.eik;
    const vatNumberControl = personForm.controls.vatNumber;

    vatNumberControl.setValue(eikControl.value, { emitEvent: false });

    eikControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((eikValue) => {
        if (vatNumberControl.value === eikValue) {
          return;
        }

        vatNumberControl.setValue(eikValue, { emitEvent: false });
      });
  }
}
