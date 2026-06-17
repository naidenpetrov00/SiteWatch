import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import {
  AddPersonContactsFormArray,
  CONTACT_TYPES,
  CONTACT_TYPE_OPTIONS
} from '../add-person-dialog.types';
import { ADD_PERSON_VALIDATION_LIMITS } from '../add-person-dialog.validators';
import { PersonPhoneInputComponent } from '../inputs/person-phone-input.component';

@Component({
  selector: 'app-add-person-contacts-section',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    PersonPhoneInputComponent
  ],
  templateUrl: './add-person-contacts-section.component.html',
  styleUrl: './add-person-contacts-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonContactsSectionComponent {
  readonly contacts = input.required<AddPersonContactsFormArray>();

  readonly addRequested = output<void>();
  readonly removeRequested = output<number>();

  readonly contactTypes = CONTACT_TYPES;
  readonly contactTypeOptions = CONTACT_TYPE_OPTIONS;
  readonly validationLimits = ADD_PERSON_VALIDATION_LIMITS;
}
