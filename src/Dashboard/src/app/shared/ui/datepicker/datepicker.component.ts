import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-datepicker',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatNativeDateModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './datepicker.component.html',
  styleUrl: './datepicker.component.css',
  providers: [provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DatepickerComponent {
  readonly control = input.required<FormControl<Date | null>>();
  readonly label = input.required<string>();
  readonly showRequiredError = input(true);
  readonly showParseError = input(true);

  selectToday(): void {
    this.control().setValue(new Date());
  }
}
