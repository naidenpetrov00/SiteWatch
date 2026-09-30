import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  RetailerExtractionRule,
  RetailerExtractionRuleType,
  SaveRetailerExtractionRuleRequest
} from '../../models/retailer-extraction-profile.models';

@Component({
  selector: 'app-extraction-rule-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './extraction-rule-dialog.component.html',
  styleUrl: './extraction-rule-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExtractionRuleDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(
    MatDialogRef<ExtractionRuleDialogComponent, SaveRetailerExtractionRuleRequest | null>
  );
  readonly rule = inject<RetailerExtractionRule | null>(MAT_DIALOG_DATA);

  readonly form = this.formBuilder.group(
    {
      name: this.formBuilder.nonNullable.control(this.rule?.name ?? '', [
        Validators.required,
        Validators.maxLength(200),
        meaningfulText()
      ]),
      isEnabled: this.formBuilder.nonNullable.control(this.rule?.isEnabled ?? true),
      ruleType: this.formBuilder.nonNullable.control<RetailerExtractionRuleType>(
        this.rule?.ruleType ?? 'jsonLd',
        Validators.required
      ),
      jsonLdObjectType: this.formBuilder.nonNullable.control(
        this.rule?.jsonLdObjectType ?? '',
        Validators.maxLength(200)
      ),
      jsonLdPricePath: this.formBuilder.nonNullable.control(
        this.rule?.jsonLdPricePath ?? '',
        Validators.maxLength(512)
      ),
      jsonLdCurrencyPath: this.formBuilder.nonNullable.control(
        this.rule?.jsonLdCurrencyPath ?? '',
        Validators.maxLength(512)
      ),
      cssSelector: this.formBuilder.nonNullable.control(
        this.rule?.cssSelector ?? '',
        Validators.maxLength(2048)
      ),
      cssValueSource: this.formBuilder.nonNullable.control<'textContent' | 'attribute'>(
        this.rule?.cssValueSource ?? 'textContent'
      ),
      cssAttributeName: this.formBuilder.nonNullable.control(
        this.rule?.cssAttributeName ?? '',
        Validators.maxLength(200)
      ),
      decimalSeparator: this.formBuilder.nonNullable.control<'dot' | 'comma'>(
        this.rule?.decimalSeparator ?? 'comma'
      ),
      thousandsSeparator: this.formBuilder.nonNullable.control<
        'none' | 'dot' | 'comma' | 'space'
      >(this.rule?.thousandsSeparator ?? 'space'),
      priceBasis: this.formBuilder.nonNullable.control<'item' | 'package'>(
        this.rule?.priceBasis ?? 'item'
      ),
      minimumValue: this.formBuilder.control<number | null>(
        this.rule?.minimumValue ?? null,
        [Validators.min(0.01), maximumTwoDecimals()]
      ),
      maximumValue: this.formBuilder.control<number | null>(
        this.rule?.maximumValue ?? null,
        [Validators.min(0.01), maximumTwoDecimals()]
      )
    },
    { validators: [ruleConfigurationValidator()] }
  );

  readonly title = this.rule ? 'Edit Extraction Rule' : 'Add Extraction Rule';
  readonly submitLabel = this.rule ? 'Save Rule' : 'Add Rule';
  readonly formId = this.rule ? 'edit-extraction-rule-form' : 'add-extraction-rule-form';

  close(): void {
    this.dialogRef.close(null);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const jsonLd = value.ruleType === 'jsonLd';
    const attribute = !jsonLd && value.cssValueSource === 'attribute';
    this.dialogRef.close({
      name: value.name.trim().replace(/\s+/g, ' '),
      isEnabled: value.isEnabled,
      ruleType: value.ruleType,
      jsonLdObjectType: jsonLd ? normalizeOptional(value.jsonLdObjectType) : null,
      jsonLdPricePath: jsonLd ? normalizeOptional(value.jsonLdPricePath) : null,
      jsonLdCurrencyPath: jsonLd ? normalizeOptional(value.jsonLdCurrencyPath) : null,
      cssSelector: jsonLd ? null : normalizeOptional(value.cssSelector),
      cssValueSource: jsonLd ? null : value.cssValueSource,
      cssAttributeName: attribute ? normalizeOptional(value.cssAttributeName) : null,
      decimalSeparator: value.decimalSeparator,
      thousandsSeparator: value.thousandsSeparator,
      expectedCurrencyCode: 'EUR',
      priceBasis: value.priceBasis,
      minimumValue: value.minimumValue,
      maximumValue: value.maximumValue
    });
  }
}

function ruleConfigurationValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as {
      ruleType?: RetailerExtractionRuleType;
      jsonLdObjectType?: string;
      jsonLdPricePath?: string;
      cssSelector?: string;
      cssValueSource?: string;
      cssAttributeName?: string;
      decimalSeparator?: string;
      thousandsSeparator?: string;
      minimumValue?: number | null;
      maximumValue?: number | null;
    };
    if (value.ruleType === 'jsonLd') {
      if (!value.jsonLdObjectType?.trim() || !value.jsonLdPricePath?.trim()) {
        return { jsonLdRequired: true };
      }
    } else if (
      !value.cssSelector?.trim() ||
      (value.cssValueSource === 'attribute' && !value.cssAttributeName?.trim())
    ) {
      return { cssRequired: true };
    }

    if (
      value.thousandsSeparator !== 'none' &&
      value.thousandsSeparator !== 'space' &&
      value.decimalSeparator === value.thousandsSeparator
    ) {
      return { separatorConflict: true };
    }
    if (
      value.minimumValue !== null &&
      value.maximumValue !== null &&
      value.minimumValue !== undefined &&
      value.maximumValue !== undefined &&
      value.minimumValue > value.maximumValue
    ) {
      return { invalidRange: true };
    }
    return null;
  };
}

function meaningfulText(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    /[\p{L}\p{N}]/u.test(String(control.value ?? ''))
      ? null
      : { meaningfulText: true };
}

function maximumTwoDecimals(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as number | null;
    if (value === null) return null;
    const scaled = value * 100;
    return Number.isFinite(value) &&
      Math.abs(scaled - Math.round(scaled)) < 0.00000001
      ? null
      : { decimalPlaces: true };
  };
}

function normalizeOptional(value: string): string | null {
  return value.trim() || null;
}
