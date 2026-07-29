---
name: dashboard-feature-architecture
description: Organize and evolve the Angular Dashboard under src/Dashboard while preserving its standalone core/features/shared boundaries. Use when adding, moving, splitting, or reviewing Dashboard pages, layouts, components, services, models, utilities, providers, or shared UI and when deciding which feature owns new code.
---

# Dashboard Feature Architecture

Keep Dashboard code cohesive without turning core or shared into catch-all folders.

## Scope and required context: establish context

1. Read the repository and src/Dashboard AGENTS.md files.
2. Inspect package.json, angular.json, tsconfig.json, app.config.ts, app.routes.ts, and representative files in the affected feature.
3. Search the requested feature and shared scope for an existing owner before creating files.
4. Inspect backend code only read-only and only when an HTTP contract or authorization seam must be confirmed.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: ownership

- Put application-wide API URL, authentication, interceptor, and query configuration in src/app/core.
- Put feature pages, feature services, serialized models, dialogs, directives, validators, mappers, and utilities in src/app/features/<feature>.
- Put UI or data infrastructure in src/app/shared only after at least two features have a concrete reuse or ownership need.
- Keep feature-specific wording, validation, columns, and request construction out of shared components.
- Keep app bootstrap providers in app.config.ts and route structure in app.routes.ts.
- Preserve standalone components, strict TypeScript, strict templates, and explicit component imports.

## Design feature slices

- Start from a routed page or other feature entry point and keep its models, services, components, and utilities beneath the owning feature.
- Depend on another feature only through the smallest existing public service or data contract. Do not import another feature's page or dialog to reuse behavior.
- Extract pure calculations and mappings from large components when they have independent behavior or focused tests.
- Split complex UI by responsibility, not by arbitrary file length. Keep orchestration in the page or dialog container and pass typed state to focused children.
- Use shared shells and controls through composition. Do not add feature switches to a shared component.

## Treat current patterns carefully

- Preserve intentional core/features/shared direction as EXPLICIT architecture.
- Prefer direct or computed signal state over copying derived state through effects. Existing effect-based synchronization is precedent to evaluate, not a rule to spread.
- Keep typed Reactive Forms as the current form architecture. Do not introduce Signal Forms without an explicit pilot or migration decision.
- Keep TanStack Query responsible for server state and native signals responsible for simple local UI state.
- Treat SSR, service workers, localization, and NgRx as opt-in capabilities; their dedicated skills own adoption.

## Related skills and repository references

- Use dashboard-components-templates for component and signal details.
- Use dashboard-routing-composition for routes, guards, and provider scopes.
- Use dashboard-models-mapping for serialized contracts and transformations.
- Use dashboard-data-access for HTTP and TanStack Query.
- Use dashboard-testing for behavioral verification.

## Verification and definition of done

- Confirm every new file has one clear owner and no dependency points from shared or core back into a feature.
- Confirm shared extraction has demonstrated cross-feature use.
- Run npm run build after implementation changes.
- Run npm test for affected behavior and stop the command if it enters watch mode.
- Do not invent a lint check; this project has no lint script.

## Context7 fallback and validated technologies

Repository architecture is authoritative. Framework guidance was validated in 2026-07 against Angular 21.2.13 and TypeScript 5.9.3. Query Context7 narrowly when a newer installed version, unfamiliar Angular API, or repository conflict makes these rules uncertain.
