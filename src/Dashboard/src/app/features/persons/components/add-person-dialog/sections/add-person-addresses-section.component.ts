import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

import { AddPersonAddressesFormArray } from '../add-person-dialog.types';

@Component({
  selector: 'app-add-person-addresses-section',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule
  ],
  templateUrl: './add-person-addresses-section.component.html',
  styleUrl: './add-person-addresses-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonAddressesSectionComponent {
  readonly addresses = input.required<AddPersonAddressesFormArray>();

  readonly addRequested = output<void>();
  readonly removeRequested = output<number>();
}
