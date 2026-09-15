import { HttpErrorResponse } from '@angular/common/http';

import { getActivityCatalogError } from './activity-catalog-error';

describe('getActivityCatalogError', () => {
  it('prefers validation, problem-detail, and legacy error messages in that order', () => {
    expect(getActivityCatalogError(new HttpErrorResponse({ error: {
      details: [{ message: 'Name is required.' }], detail: 'Conflict.'
    } }))).toBe('Name is required.');
    expect(getActivityCatalogError(new HttpErrorResponse({ error: { detail: 'Conflict.' } }))).toBe('Conflict.');
    expect(getActivityCatalogError(new HttpErrorResponse({ error: { errorMessage: 'Legacy failure.' } }))).toBe('Legacy failure.');
  });

  it('returns the supplied fallback for unrecognized failures', () => {
    expect(getActivityCatalogError(new Error('offline'), 'Try again.')).toBe('Try again.');
  });
});
