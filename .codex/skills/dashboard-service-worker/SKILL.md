---
name: dashboard-service-worker
description: Assess and implement opt-in Angular service-worker and PWA behavior for src/Dashboard, including package setup, ngsw-config caching, registration, updates, offline behavior, authenticated-data safety, deployment recovery, and production verification. Use only when a task explicitly requests a service worker, PWA installation, offline support, caching, or update notifications.
---

# Dashboard Service Worker

Treat a service worker as persistent production infrastructure, not a harmless frontend toggle.

## Scope and required context: gate adoption

- Confirm the exact offline, installability, performance, or update requirement.
- Verify package.json and package-lock.json. @angular/service-worker is currently not installed.
- Ask before adding dependencies or running ng add @angular/pwa.
- Select a package version compatible with Angular and CLI 21.2.x rather than using latest blindly.
- Review every generated or modified package, angular.json, index.html, manifest, icon, provider, and ngsw-config change.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: capability selection

- Use Angular's service worker for basic application-shell and simple data caching.
- Recognize that official Angular guidance describes it as a limited basic caching utility receiving security fixes rather than new features.
- Prefer native service-worker APIs only when approved requirements exceed Angular's supported feature set; that is a separate architecture decision.
- Keep the application fully usable when service workers are unsupported or disabled.

## Configure safely

- Register with provideServiceWorker and keep it disabled in normal development unless an explicit test requires production behavior.
- Prefer registration after application stability unless a measured requirement justifies another strategy.
- Define assetGroups and dataGroups narrowly.
- Classify each resource by sensitivity, freshness, maximum age, timeout, and offline value.
- Do not cache sign-in, tokens, authorization responses, user-specific private data, or broad authenticated API patterns by default.
- Prefer network behavior for mutable or sensitive Dashboard data. Add a data cache only after an explicit privacy and staleness review.
- Keep locale-specific files and base paths aligned with dashboard-internationalization.

## Handle versions and updates

- Use SwUpdate only when isEnabled is true.
- Observe versionUpdates and present a deliberate refresh action when a new compatible application version is ready.
- Do not force activation mid-session without assessing state loss and version skew.
- Remember that open tabs can continue running the old coherent version while the new version downloads.
- Document deployment recovery, including safe removal or replacement of a broken worker.

## Verification and definition of done

- Run npm run build and confirm the approved build emits the worker manifest and expected resources.
- Inspect ngsw-config patterns and generated ngsw.json rather than assuming generated defaults cover custom assets.
- Test first install, offline shell, update discovery, refresh behavior, disabled support, sensitive-request bypass, and rollback.
- Service-worker runtime verification requires HTTPS or localhost and an explicitly approved server/browser session. Do not start one by default.
- Use a clean or private browser context to avoid stale registrations during approved testing.

## Related skills and repository references

Use dashboard-data-access for API sensitivity, dashboard-authentication for private requests, dashboard-internationalization for locale assets, and dashboard-testing for policy and provider tests.

## Context7 fallback and validated technologies

Validated in 2026-07 against Angular 21.2.13 and current official Angular service-worker guidance. @angular/service-worker is not installed, so its exact version remains unknown until adoption. Revalidate package and configuration behavior with Context7 and official docs before implementation.
