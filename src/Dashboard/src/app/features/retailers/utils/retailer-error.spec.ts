import { HttpErrorResponse } from '@angular/common/http';

import { getRetailerError } from './retailer-error';

describe('getRetailerError', () => {
  it('prefers validation and conflict details over the fallback message', () => {
    expect(getRetailerError(new HttpErrorResponse({ status: 400, error: { details: [{ message: 'BaseWebsiteUrl must be valid.' }] } }))).toBe('BaseWebsiteUrl must be valid.');
    expect(getRetailerError(new HttpErrorResponse({ status: 409, error: { detail: 'A retailer already represents this website host.' } }))).toBe('A retailer already represents this website host.');
  });

  it('returns the supplied fallback for unknown errors', () => {
    expect(getRetailerError(new Error('offline'), 'Retailers are unavailable.')).toBe('Retailers are unavailable.');
  });
});
