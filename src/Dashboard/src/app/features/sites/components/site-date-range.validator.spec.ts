import { FormControl, FormGroup } from '@angular/forms';

import { siteDateRangeValidator } from './site-date-range.validator';

describe('siteDateRangeValidator', () => {
  it('accepts an equal start and end date boundary', () => {
    const form = createForm(new Date(2026, 2, 3), new Date(2026, 2, 3));

    expect(siteDateRangeValidator()(form)).toBeNull();
  });

  it('accepts an incomplete date range', () => {
    const form = createForm(new Date(2026, 2, 3), null);

    expect(siteDateRangeValidator()(form)).toBeNull();
  });

  it('rejects an end date before the start date', () => {
    const form = createForm(new Date(2026, 2, 3), new Date(2026, 2, 2));

    expect(siteDateRangeValidator()(form)).toEqual({ dateRange: true });
  });

  function createForm(startDate: Date | null, endDate: Date | null): FormGroup {
    return new FormGroup({ startDate: new FormControl(startDate), endDate: new FormControl(endDate) });
  }
});
