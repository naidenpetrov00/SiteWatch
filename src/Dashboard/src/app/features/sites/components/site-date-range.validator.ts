import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

type SiteDateRange = {
  startDate?: Date | null;
  endDate?: Date | null;
};

export function siteDateRangeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const { startDate, endDate } = (control.value as SiteDateRange | null) ?? {};

    return startDate instanceof Date && endDate instanceof Date && endDate < startDate
      ? { dateRange: true }
      : null;
  };
}
