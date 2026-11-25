import {describe, it, expect} from 'vitest';
import {canonicalRedirectOptions, resolveCanonicalRedirect, type CanonicalRedirectInput} from '../canonical-redirect';

const CANONICAL = 'ado-auditor.com';

const input = (overrides: Partial<CanonicalRedirectInput> = {}): CanonicalRedirectInput => ({
  shouldRedirect: true,
  redirectToUrl: CANONICAL,
  host: CANONICAL,
  forwardedProto: 'https',
  fullPath: '/',
  ...overrides,
});

describe('resolveCanonicalRedirect', () => {
  describe('when it should stay out of the way', () => {
    it('returns null if the request already matches the canonical host over https', () => {
      expect(resolveCanonicalRedirect(input())).toBeNull();
    });

    it('returns null if redirecting is switched off', () => {
      expect(resolveCanonicalRedirect(input({shouldRedirect: false, host: `www.${CANONICAL}`}))).toBeNull();
    });

    it('returns null if the switch is missing entirely', () => {
      // An unset NUXT_SHOULD_REDIRECT must leave traffic alone.
      expect(resolveCanonicalRedirect(input({shouldRedirect: undefined, host: `www.${CANONICAL}`}))).toBeNull();
    });

    it.each([
      ['the string "true"', 'true'],
      ['the number 1', 1],
      ['null', null],
    ])('returns null for %s, because only a strict true counts as on', (_label, value) => {
      expect(resolveCanonicalRedirect(input({shouldRedirect: value, host: `www.${CANONICAL}`}))).toBeNull();
    });

    it.each([
      ['an empty target', ''],
      ['a missing target', undefined],
    ])('returns null for %s rather than inventing a destination', (_label, redirectToUrl) => {
      expect(resolveCanonicalRedirect(input({redirectToUrl, host: `www.${CANONICAL}`, forwardedProto: 'http'}))).toBeNull();
    });
  });

  describe('when the scheme needs upgrading', () => {
    it('redirects http to https', () => {
      expect(resolveCanonicalRedirect(input({forwardedProto: 'http', fullPath: '/secure'})))
        .toBe(`https://${CANONICAL}/secure`);
    });

    it('keeps the host the caller asked for while upgrading the scheme', () => {
      // Moving scheme and host in one step would guess which host was meant. The second
      // request, now on https, is where the host gets corrected.
      expect(resolveCanonicalRedirect(input({host: `www.${CANONICAL}`, forwardedProto: 'http', fullPath: '/secure'})))
        .toBe(`https://www.${CANONICAL}/secure`);
    });

    it('treats a missing protocol header as http', () => {
      // Behind a proxy that does not set the header, assuming https would leave plain
      // http traffic untouched.
      expect(resolveCanonicalRedirect(input({forwardedProto: undefined, fullPath: '/assume-http'})))
        .toBe(`https://${CANONICAL}/assume-http`);
    });

    it('falls back to the canonical host when the request carries no host either', () => {
      expect(resolveCanonicalRedirect(input({host: undefined, forwardedProto: undefined, fullPath: '/no-host'})))
        .toBe(`https://${CANONICAL}/no-host`);
    });
  });

  describe('when the host needs correcting', () => {
    it('redirects a www host to the canonical one', () => {
      expect(resolveCanonicalRedirect(input({host: `www.${CANONICAL}`, fullPath: '/redirect-me'})))
        .toBe(`https://${CANONICAL}/redirect-me`);
    });

    it('redirects any other host to the canonical one', () => {
      expect(resolveCanonicalRedirect(input({host: 'ado-exporter.com', fullPath: '/old-domain'})))
        .toBe(`https://${CANONICAL}/old-domain`);
    });

    it('carries the path and the query string across', () => {
      expect(resolveCanonicalRedirect(input({host: `www.${CANONICAL}`, fullPath: '/audit/42?tab=pats&sort=severity'})))
        .toBe(`https://${CANONICAL}/audit/42?tab=pats&sort=severity`);
    });

    it('treats the host comparison as case sensitive, matching the header as received', () => {
      // Documents the current behaviour rather than claiming it is ideal: a request on
      // ADO-AUDITOR.COM is redirected once to the lowercase host.
      expect(resolveCanonicalRedirect(input({host: CANONICAL.toUpperCase(), fullPath: '/casing'})))
        .toBe(`https://${CANONICAL}/casing`);
    });

    it('does not redirect a host that only differs by port', () => {
      // A host with a port is a different string, so it is redirected. Worth knowing
      // before putting this in front of anything that is not on 443.
      expect(resolveCanonicalRedirect(input({host: `${CANONICAL}:8080`, fullPath: '/ported'})))
        .toBe(`https://${CANONICAL}/ported`);
    });
  });
});

// The middleware body runs on the server only and cannot be reached from a test, so the
// way it calls navigateTo is pinned here. With { redirectCode: 301 } alone every
// redirect ended in a 500.
describe('canonicalRedirectOptions', () => {
  const target = resolveCanonicalRedirect(input({host: `www.${CANONICAL}`, fullPath: '/redirect-me'})) as string;

  it('is needed, because navigateTo refuses an absolute URL that is not marked external', () => {
    // E2001 is Nuxt's diagnostic for "navigating to an external URL is not allowed by default".
    expect(() => navigateTo(target, {redirectCode: 301})).toThrow(/e2001/i);
  });

  it('marks the target as external and keeps the redirect permanent', () => {
    expect(canonicalRedirectOptions).toEqual({redirectCode: 301, external: true});
  });
});
