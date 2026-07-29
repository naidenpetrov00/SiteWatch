---
name: dashboard-testing
description: Add focused Angular Dashboard tests with the Angular unit-test builder and Vitest for standalone components, signals, templates, services, validators, mappers, HttpClient, interceptors, route guards, routing, Angular Material, TanStack Query, storage, and opt-in platform features. Use whenever Dashboard behavior changes or a regression needs coverage.
---

# Dashboard Testing

Test behavior at the smallest useful boundary with the providers the production code actually needs.

## Scope and required context: respect project tooling

- Use npm test as the confirmed unit-test command.
- Stop the process if it enters watch mode; do not leave long-running tests active.
- Use npm run build as the confirmed production compilation check.
- Do not invent or claim a lint check; package.json defines none.
- Do not add an E2E framework, test dependency, or alternate runner without approval.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: test design

- Test validators, request mappers, calculations, JWT utilities, formatters, table normalization, and equality as plain functions when Angular injection is unnecessary.
- Cover normal, empty, boundary, malformed, and error cases that can change observable behavior.
- Avoid tests that merely repeat TypeScript types.

## Test standalone components

- Import the standalone component into TestBed.
- Use fixture.detectChanges for deliberate synchronous initial rendering when appropriate.
- Await fixture.whenStable after signal-driven, router, harness, or other asynchronous work.
- Assert user-visible behavior, inputs, outputs, accessible names, disabled states, and error handling.
- Use setInput for signal inputs when it better represents Angular binding.
- Prefer Material harnesses when a Material component's internal DOM is not part of the contract.
- Do not adopt a categorical ban on detectChanges from a generic skill; match the actual test environment and behavior.

## Test services and integration seams

- Register provideHttpClient before provideHttpClientTesting.
- Use HttpTestingController to assert method, URL, headers, parameters, body, response, and error behavior.
- Provide a fresh QueryClient per test suite and prevent cache leakage between tests.
- Test mutation invalidation and reactive query-key changes where they are the behavior.
- Clear sessionStorage around authentication tests.
- Run functional interceptors and guards inside a real injection context.
- Prefer RouterTestingHarness and test routes over mocking Router for navigation flows.

## Target meaningful coverage

- For forms, test validator logic, conditional changes, invalid submit, pending state, success, and failure.
- For tables, test state transitions, filters, sorting, paging, counts, actions, and export scope.
- For authentication, test restore, expiry, bypass, bearer headers, 401, 403 policy, login, logout, and redirects.
- For async UI, test pending, success, empty, error, retry, background refresh, and mutation outcomes.
- For opt-in capabilities, test provider configuration and pure policy before any runtime infrastructure verification.

## Related skills and repository references

Compose with the skill for the behavior under test. Use `package.json`, `angular.json`, existing `*.spec.ts` files, and the nearest `AGENTS.md` as repository references.

## Verification and definition of done

Run the smallest relevant test scope supported by existing commands, then npm run build for implementation changes. Report commands that cannot be run. Do not start the Dashboard server, browser, SSR server, service worker host, or external infrastructure without explicit approval.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular build and CLI 21.2.11, Angular 21.2.13, Vitest 4.1.6, and jsdom 28.1.0. Query Context7 for changed Angular testing, harness, or Vitest integration behavior.
