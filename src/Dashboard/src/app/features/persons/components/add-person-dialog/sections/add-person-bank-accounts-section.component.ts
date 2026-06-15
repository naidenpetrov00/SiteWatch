import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

import { AddPersonBankAccountsFormArray } from '../add-person-dialog.types';

@Component({
  selector: 'app-add-person-bank-accounts-section',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule
  ],
  templateUrl: './add-person-bank-accounts-section.component.html',
  styleUrl: './add-person-bank-accounts-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonBankAccountsSectionComponent {
  readonly bankAccounts = input.required<AddPersonBankAccountsFormArray>();

  readonly addRequested = output<void>();
  readonly removeRequested = output<number>();
}
