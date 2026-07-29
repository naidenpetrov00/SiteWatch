---
name: dashboard-ssr-hydration
description: Assess and implement opt-in server-side, static, hybrid rendering, and Angular client hydration for src/Dashboard, including SSR prerequisites, provider symmetry, event replay, incremental hydration, browser-only APIs, DOM parity, i18n hydration, and verification. Use only when a task explicitly requests SSR, SSG, hybrid rendering, hydration, or hydration debugging.
---

# Dashboard SSR and Hydration

Keep client-side rendering as the Dashboard default. Adopt hydration only as part of an explicitly approved rendering architecture.

## Scope and required context: gate adoption

- Confirm the product reason for SSR, SSG, hybrid rendering, or hydration. An authenticated internal dashboard usually does not gain enough SEO value by default.
- Verify package.json and package-lock.json. @angular/ssr is currently not installed.
- Ask before adding or changing a dependency, running ng add @angular/ssr, or changing deployment and server requirements.
- Resolve an @angular/ssr version compatible with Angular 21.2.13; do not use latest blindly.
- Treat server startup and browser runtime verification as separate actions requiring explicit approval.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: SSR before hydration

- Do not add provideClientHydration to a browser-only build and call the work complete.
- Configure SSR or hybrid rendering first through supported Angular tooling.
- Include hydration providers in both client and server bootstrap paths.
- Use provideClientHydration as the base provider.
- Add withEventReplay when pre-hydration interactions must be retained.
- Use incremental hydration only for a demonstrated route or defer-block performance need.
- Add withI18nSupport when localized i18n blocks must hydrate.

## Keep rendering deterministic

- Produce the same valid DOM structure on the server and client, including table structure and relevant whitespace settings.
- Avoid direct DOM mutation before hydration. Prefer Angular templates and rendering APIs.
- Treat ngSkipHydration as a temporary component-host workaround, not a permanent architecture.
- Do not branch rendered markup with isPlatformBrowser merely to avoid a browser API; that creates layout differences.
- Defer client-only integrations until after rendering when necessary.
- Keep browser storage, window, document, and other browser APIs behind safe guards. The existing auth session guard is relevant precedent.
- Ensure timers, unresolved promises, and perpetual tasks do not prevent application stability.

## Integrate state carefully

- Do not fetch the same initial data independently on server and client without deciding how it transfers.
- Keep TanStack Query hydration or cache transfer version-specific and validate the installed adapter through Context7.
- Do not serialize access tokens or user-private cache data into shared HTML.
- Coordinate locale rendering with dashboard-internationalization and cached resources with dashboard-service-worker.

## Related skills and repository references

Compose with `dashboard-routing-composition`, `dashboard-data-access`, `dashboard-authentication`, `dashboard-internationalization`, and `dashboard-testing`. Inspect the current CSR bootstrap and browser-only APIs as repository evidence.

## Verification and definition of done

- Run npm run build after approved implementation.
- Test server and client DOM parity, browser-only API safety, authentication behavior, localized blocks, navigation, and hydration error handling.
- Runtime SSR and browser checks require explicit authorization; do not start servers by default.
- Use Angular DevTools or dev-mode hydration diagnostics when runtime verification is approved.

## Context7 fallback and validated technologies

Validated in 2026-07 against Angular 21.2.13 project evidence and current official Angular hydration guidance. @angular/ssr is not installed, so its exact version is unknown until adoption. Query Context7 and official Angular documentation for the selected version before implementation.
