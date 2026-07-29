---
name: dashboard-data-access
description: Implement and evolve Dashboard HTTP clients and TanStack Angular Query server-state flows, including typed requests, reactive query options, query keys, filtering parameters, mutations, invalidation, API URL handling, and server/client-state boundaries. Use for feature service, HttpClient, injectQuery, injectMutation, QueryClient, or core query configuration changes.
---

# Dashboard Data Access

Keep remote server state typed, cacheable, and owned by feature services.

## Scope and required context: inspect the full contract

1. Read the feature service, models, consuming page or dialog, core query config, and API URL helper.
2. Inspect the corresponding API endpoint and DTO read-only when routes, parameters, authorization, or response shapes are unclear.
3. Confirm exact installed Angular and TanStack Query versions before using an unfamiliar API.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: feature services

- Keep feature HTTP and server-state orchestration in an @Injectable service under the owning feature.
- Inject HttpClient and QueryClient with inject().
- Build relative application URLs through buildApiUrl; do not duplicate origins or environment assumptions.
- Use typed request and response models from the feature.
- Convert finite HttpClient observables to promises with firstValueFrom when required by the current TanStack adapter.
- Keep direct one-shot lookups explicit and promote them to cached queries only when cache identity and lifecycle are useful.

## Define queries

- Pass a reactive options factory to injectQuery and read every signal that affects the request inside that factory.
- Use hierarchical query-key arrays with a stable feature prefix.
- Include every paging, sorting, filter, identifier, and locale input that can change the result.
- Normalize filters before generating both HttpParams and the query-key state.
- Keep key construction deterministic; sort record entries before serializing them.
- Do not mirror query data into writable component signals merely to display it.
- Keep global defaults in core/query/tanstack-query.ts and feature-specific behavior with the feature query.

## Define mutations

- Give mutations stable descriptive keys.
- Make mutation functions return the real typed result.
- Invalidate the narrow query-key prefix whose cached data became stale.
- Await invalidation when subsequent UI behavior depends on refreshed state.
- Keep navigation, dialog closing, and user messaging at the UI boundary unless they are intrinsic to the data operation.
- Do not implement a second cache in NgRx or writable signals.

## Preserve boundaries

- TanStack Query owns remote server state and freshness.
- Native signals own simple local UI state.
- NgRx owns only approved complex client or workflow state.
- dashboard-models-mapping owns serialization and transformation.
- dashboard-async-ui-state owns pending, error, empty, and retry presentation.
- dashboard-authentication owns bearer headers and public-request bypass.

## Avoid

- Do not add a dependency, proxy, environment system, or code generator without approval.
- Do not put secrets in client configuration.
- Do not use unbounded queries or client-side sorting to simulate server paging.
- Do not invent cancellation wiring for an adapter API without verifying its supported mechanism.

## Related skills and repository references

Compose with `dashboard-models-mapping`, `dashboard-async-ui-state`, `dashboard-authentication`, and `dashboard-testing`. Inspect the matching backend endpoint and Application contract as repository evidence without broadening changes beyond `src/Dashboard`.

## Verification and definition of done

- Test URL, parameters, query-key changes, mutation payloads, invalidation, success, and error behavior.
- Use HttpTestingController and an isolated QueryClient through dashboard-testing.
- Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular HTTP 21.2.13, TanStack Angular Query 5.101.0, and RxJS 7.8.2. Context7 v5 Angular guidance was not version-pinned to 5.101.0; query it narrowly for uncovered or changed adapter APIs.
