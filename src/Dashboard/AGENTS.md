# Dashboard Instructions

These rules supplement the repository root instructions for `src/Dashboard`.

## Scope and structure

- `src/app/core` contains application-wide API, authentication, and query infrastructure. `src/app/features` owns feature-specific pages, services, models, and components. `src/app/shared` contains reusable UI and data-table building blocks.
- Keep feature-specific behavior within its feature. Move code to `shared` only for a concrete cross-feature reuse or ownership need.
- Preserve the strict TypeScript and Angular template settings in `tsconfig.json`. Follow the existing standalone Angular organization and use applicable Angular skills from the active skill catalog for implementation details.

## Package and generated files

- Use npm and preserve `package-lock.json` as the lockfile.
- Treat `dist`, `.angular`, coverage, and other compiled or cached output as generated; do not edit them directly.
- Prettier is configured, but no repository formatting script exists. Avoid repository-wide formatting or broad autofix churn.

## Commands and execution boundaries

- `npm run build` is the confirmed production build check for dashboard changes.
- `npm test` is the confirmed unit-test command. Run relevant existing tests after behavioral changes; if the command enters watch mode, stop it rather than leaving it running.
- `npm start` and `npm run watch` start long-running development processes and require an explicit request.
- No lint script is defined. Do not invent or claim a lint check; report that limitation when relevant.
