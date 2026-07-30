# Dashboard Instructions

These rules supplement the repository root instructions for `src/Dashboard`.

## Scope and structure

- `src/app/core` contains application-wide API, authentication, and query infrastructure. `src/app/features` owns feature-specific pages, services, models, and components. `src/app/shared` contains reusable UI and data-table building blocks.
- Keep feature-specific behavior within its feature. Move code to `shared` only for a concrete cross-feature reuse or ownership need.
- Preserve strict TypeScript, strict templates, standalone components, and explicit component imports.
- Keep TanStack Query responsible for remote state and Angular signals responsible for local UI state; do not create a second cache.
- Keep typed Reactive Forms as the current form architecture. Reuse the shared dialog shell and Material 3 system-token styling when their contracts fit.
- Use applicable Angular skills for framework procedures that are not SiteWatch-specific.

## Package and generated files

- Use npm and preserve `package-lock.json` as the lockfile.
- Treat `dist`, `.angular`, coverage, and other compiled or cached output as generated; do not edit them directly.
- Prettier is configured, but no repository formatting script exists. Avoid repository-wide formatting or broad autofix churn.
- No Dashboard lint script is defined; do not invent one.
- Before adding an absent opt-in capability such as SSR, service workers, localization, or NgRx, obtain dependency approval and select a version compatible with the installed Angular and CLI versions.
