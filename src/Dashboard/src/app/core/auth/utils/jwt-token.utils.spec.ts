import { decodeJwtPayload, isTokenExpired } from './jwt-token.utils';

function createJwt(payload: Record<string, unknown>): string {
  const header = toBase64Url(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = toBase64Url(JSON.stringify(payload));
  return `${header}.${body}.`;
}

function toBase64Url(value: string): string {
  return btoa(value).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '');
}

describe('jwt-token.utils', () => {
  it('decodes a JWT payload', () => {
    const token = createJwt({ exp: 123, sub: 'user-1' });

    expect(decodeJwtPayload(token)).toEqual(
      expect.objectContaining({ exp: 123, sub: 'user-1' })
    );
  });

  it('detects expired tokens', () => {
    const token = createJwt({ exp: Math.floor(Date.now() / 1000) - 10 });

    expect(isTokenExpired(token)).toBeTrue();
  });

  it('treats malformed tokens as non-expired', () => {
    expect(decodeJwtPayload('bad-token')).toBeNull();
    expect(isTokenExpired('bad-token')).toBeFalse();
  });
});
