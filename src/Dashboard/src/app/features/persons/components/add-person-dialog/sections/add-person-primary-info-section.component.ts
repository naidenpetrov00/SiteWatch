import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

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
export class AddPersonPrimaryInfoSectionComponent {
  readonly personForm = input.required<AddPersonDialogFormGroup>();
  readonly personTypeOptions = PERSON_TYPE_OPTIONS;
  readonly vatCountryCodeOptions = VAT_COUNTRY_CODE_OPTIONS;
}
