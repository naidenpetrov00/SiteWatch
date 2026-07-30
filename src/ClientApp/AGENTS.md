# Expo Client Instructions

These rules supplement the repository root instructions for `src/ClientApp`.

## Scope and structure

- `app` owns Expo Router routes and layouts. Put reusable feature behavior in `features`; keep cross-feature UI and utilities in `components`, `hooks`, `lib`, `store`, and `types` according to the existing organization.
- Prefer the `@/*` path alias configured by `tsconfig.json` for repository-local imports where it keeps imports clear.
- Use applicable Expo and React Native skills from the active skill catalog for UI, navigation, native behavior, performance, and data fetching. Keep their implementation procedures out of this file.
- On blur, unmount, inactive, or background transitions, remove listeners and clear timers; release native resources and issue one idempotent PTZ stop when movement is active. Guard late asynchronous completions and use bounded visible recovery for interruptions.

## Package and configuration policy

- Use npm for this application because `package-lock.json` is the approved lockfile. Do not create or update a Yarn lockfile even though `package.json` currently contains Yarn package-manager metadata.
- Preserve strict TypeScript settings. Keep environment-specific API configuration in ignored local configuration; never expose values from `.env` or `app.config.ts`.
- Treat `.expo`, `dist`, `web-build`, `android`, `ios`, Gradle/CMake output, and generated Expo type files as generated or local output unless a task explicitly requests native project generation or native source work.
