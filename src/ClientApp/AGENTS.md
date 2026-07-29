# Expo Client Instructions

These rules supplement the repository root instructions for `src/ClientApp`.

## Scope and structure

- `app` owns Expo Router routes and layouts. Put reusable feature behavior in `features`; keep cross-feature UI and utilities in `components`, `hooks`, `lib`, `store`, and `types` according to the existing organization.
- Prefer the `@/*` path alias configured by `tsconfig.json` for repository-local imports where it keeps imports clear.
- Use applicable Expo and React Native skills from the active skill catalog for UI, navigation, native behavior, performance, and data fetching. Keep their implementation procedures out of this file.

## Package and configuration policy

- Use npm for this application because `package-lock.json` is the approved lockfile. Do not create or update a Yarn lockfile even though `package.json` currently contains Yarn package-manager metadata.
- Preserve strict TypeScript settings. Keep environment-specific API configuration in ignored local configuration; never expose values from `.env` or `app.config.ts`.
- Treat `.expo`, `dist`, `web-build`, `android`, `ios`, Gradle/CMake output, and generated Expo type files as generated or local output unless a task explicitly requests native project generation or native source work.

## Commands and cost boundaries

- `npm run lint` is the normal non-destructive lint check for relevant source changes.
- `npm test` runs Jest in watch mode. Do not start it automatically or leave it running; use it only when the user explicitly requests watch-mode testing.
- The established Android run command is `npx expo run:android`. It performs a slow native build/run and must not be executed unless the user explicitly tells you to run it. Do not substitute another Android build command as routine verification.
- `npm run start`, `npm run ios`, and `npm run web` start interactive or long-running tooling and require an explicit request.
- `npm run reset-project` restructures application source and must never be run without an explicit request.
- Do not use a native build to verify ordinary TypeScript, UI, or data-fetching changes. Report when lint or other available targeted checks are insufficient.
