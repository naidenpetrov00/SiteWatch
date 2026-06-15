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
  CONTACT_TYPE_OPTIONS
} from '../add-person-dialog.types';

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
    MatSelectModule
  ],
  templateUrl: './add-person-contacts-section.component.html',
  styleUrl: './add-person-contacts-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonContactsSectionComponent {
  readonly contacts = input.required<AddPersonContactsFormArray>();

  readonly addRequested = output<void>();
  readonly removeRequested = output<number>();

  readonly contactTypeOptions = CONTACT_TYPE_OPTIONS;
}
