import {
  AddPersonPhoneCountryCodeOption,
  PHONE_COUNTRY_CODE_OPTIONS
} from '../add-person-dialog.types';
import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  signal
} from '@angular/core';
import {
  ControlValueAccessor,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';
import {
  combinePhoneValue,
  sanitizePhoneNationalNumber,
  splitPhoneValue
} from '../add-person-dialog.input-utils';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { PersonInputSanitizerDirective } from '../directives/person-input-sanitizer.directive';

@Component({
  selector: 'app-person-phone-input',
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PersonInputSanitizerDirective
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PersonPhoneInputComponent),
      multi: true
    }
  ],
  templateUrl: './person-phone-input.component.html',
  styleUrl: './person-phone-input.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PersonPhoneInputComponent implements ControlValueAccessor {
  readonly countryCodeOptions = PHONE_COUNTRY_CODE_OPTIONS;
  readonly selectedCountryCode = signal<AddPersonPhoneCountryCodeOption>(
    PHONE_COUNTRY_CODE_OPTIONS[0].value
  );
  readonly nationalNumber = signal('');
  readonly isDisabled = signal(false);

  private hasSelectedCountryCode = false;
  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string | null): void {
    const normalizedValue = value ?? '';

    if (normalizedValue.length === 0) {
      this.nationalNumber.set('');

      if (!this.hasSelectedCountryCode) {
        this.selectedCountryCode.set(PHONE_COUNTRY_CODE_OPTIONS[0].value);
      }

      return;
    }

    const { countryCode, nationalNumber } = splitPhoneValue(normalizedValue);

    this.selectedCountryCode.set(countryCode);
    this.nationalNumber.set(nationalNumber);
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  handleCountryCodeChange(countryCode: AddPersonPhoneCountryCodeOption): void {
    this.hasSelectedCountryCode = true;
    this.selectedCountryCode.set(countryCode);
    this.emitValue();
  }

  handleNationalNumberInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const sanitizedNationalNumber = sanitizePhoneNationalNumber(input.value);

    if (input.value !== sanitizedNationalNumber) {
      input.value = sanitizedNationalNumber;
    }

    this.nationalNumber.set(sanitizedNationalNumber);
    this.emitValue();
  }

  handleTouched(): void {
    this.onTouched();
  }

  private emitValue(): void {
    this.onChange(combinePhoneValue(this.selectedCountryCode(), this.nationalNumber()));
  }
}
