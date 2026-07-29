---
name: dashboard-ngrx-state
description: Assess and implement opt-in NgRx client and workflow state for src/Dashboard, choosing between native signals, NgRx SignalStore, and Store with Effects while preserving TanStack Query server-state ownership. Use only when a Dashboard task explicitly requests NgRx or when client state has outgrown local signals across components, routes, or workflows.
---

# Dashboard NgRx State

Add NgRx only when state complexity justifies a new dependency and ownership model.

## Scope and required context: gate adoption

- Describe the state, its lifetime, writers, readers, transitions, side effects, debugging needs, and persistence requirements.
- Verify package.json and package-lock.json. No NgRx package is currently installed.
- Ask before adding @ngrx/signals, @ngrx/store, @ngrx/effects, or related packages.
- Resolve a version explicitly compatible with Angular 21.2.13. Context7 guidance was not version-pinned.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: state ownership

- Use component signals for simple local UI state.
- Use a feature service with signals for modest shared state with a clear imperative API.
- Prefer NgRx SignalStore for complex feature-scoped client state, derived state, methods, and lifecycle that remain signal-oriented.
- Use Store with Effects for app-wide event-driven workflows requiring explicit actions, reducers, selectors, effects, or Redux-style tooling.
- Do not adopt NgRx merely to hold an HTTP response.

## Preserve the server-state boundary

- Keep TanStack Query responsible for remote data, cache freshness, refetching, retries, and mutation invalidation.
- Do not duplicate query results into NgRx as a second cache.
- Store client-only selection, drafts, workflow progress, preferences, or coordination state when those needs are proven.
- Call feature data-access services from approved effect or store-method boundaries rather than duplicating HttpClient contracts inside state definitions.

## Implement SignalStore

- Define record-shaped state with explicit initial values.
- Use computed state for derivation and methods with patchState for controlled transitions.
- Provide the store at the component or route for feature lifetime; use root only for truly application-wide state.
- Keep side effects in explicit methods, hooks, RxJS interop, or the Events plugin as required.
- Prefer the functional SignalStore style recommended by current NgRx guidance.

## Implement Store and Effects

- Register the root store once and feature state through provideState.
- Model actions as events, keep reducers pure, and use selectors for derivation.
- Put external I/O and event-to-event orchestration in Effects.
- Register effects through standalone providers.
- Keep state immutable and serializable when Redux-style tooling or persistence depends on it.
- Avoid action chains that hide ownership or use effects as general service methods.

## Handle persistence and rendering

- Persist only explicitly approved non-sensitive client state.
- Guard browser storage for SSR and hydration.
- Never persist tokens through a generic NgRx persistence mechanism.
- Define reset behavior for logout, route teardown, and incompatible application versions.

## Related skills and repository references

Compose with `dashboard-data-access`, `dashboard-async-ui-state`, `dashboard-routing-composition`, and `dashboard-testing`. Treat existing TanStack Query services and feature-local signals as repository evidence before adding NgRx.

## Verification and definition of done

- Test state transitions, computed values or selectors, effect success and failure, provider scope, teardown, logout reset, and the TanStack boundary.
- Use TestBed for injected SignalStore features and effects.
- Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 against Angular 21.2.13, current official NgRx guidance, and the approved Dashboard state boundary. NgRx is not installed and its exact version is unknown. Revalidate package compatibility and selected APIs with Context7 and official NgRx documentation before implementation.
