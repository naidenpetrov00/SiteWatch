import { mapAuthErrors, mapDashboardSignInResult } from './auth-response.mapper';

describe('auth-response.mapper', () => {
  it('maps a successful sign-in response', () => {
    expect(
      mapDashboardSignInResult({
        result: { succeeded: true, errors: [] },
        token: 'jwt-token'
      })
    ).toEqual({
      succeeded: true,
      errors: []
    });
  });

  it('maps validation errors from the response payload', () => {
    expect(
      mapDashboardSignInResult({
        result: { succeeded: false, errors: ['Invalid credentials'] }
      })
    ).toEqual({
      succeeded: false,
      errors: ['Invalid credentials']
    });
  });

  it('falls back for empty or malformed payloads', () => {
    expect(mapDashboardSignInResult(null)).toEqual({
      succeeded: false,
      errors: ['Unable to sign in.']
    });
    expect(mapAuthErrors({})).toEqual(['Unable to sign in.']);
    expect(mapAuthErrors(['first', 2, 'second'])).toEqual(['first', 'second']);
  });

  it('maps string errors from transport failures', () => {
    expect(mapAuthErrors('Network failed')).toEqual(['Network failed']);
  });
});
