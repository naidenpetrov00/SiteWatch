import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

import { OfferActivityMeasurementUnit } from '../models/offer.models';

const MEASUREMENT_LABELS: Record<OfferActivityMeasurementUnit, string> = {
  piece: 'piece',
  cm: 'cm',
  m: 'm',
  m2: 'm²',
  m3: 'm³'
};

const PACKAGE_LABELS: Readonly<Record<string, string>> = {
  piece: 'piece',
  pack: 'pack',
  box: 'box',
  set: 'set',
  kg: 'kg',
  g: 'g',
  l: 'l',
  ml: 'ml',
  m: 'm',
  cm: 'cm',
  m2: 'm²',
  m3: 'm³',
  roll: 'roll',
  bag: 'bag'
};

export function fourDecimalPlaces(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = Number(control.value);
    if (!Number.isFinite(value)) return { decimalScale: true };
    const scaled = value * 10_000;
    return Math.abs(scaled - Math.round(scaled)) < 0.000001
      ? null
      : { decimalScale: true };
  };
}

export function formatOfferQuantity(value: number): string {
  return new Intl.NumberFormat(undefined, {
    maximumFractionDigits: 8,
    minimumFractionDigits: 0,
    useGrouping: true
  }).format(value);
}

export function measurementUnitLabel(
  unit: OfferActivityMeasurementUnit
): string {
  return MEASUREMENT_LABELS[unit];
}

export function packageUnitLabel(unit: string): string {
  return PACKAGE_LABELS[unit] ?? unit;
}
