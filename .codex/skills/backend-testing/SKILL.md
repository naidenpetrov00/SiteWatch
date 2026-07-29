---
name: backend-testing
description: Add focused backend tests for domain behavior, Application validators/use cases, mappings, queries, authorization, and Infrastructure adapters while respecting execution boundaries. Use whenever backend behavior changes or a regression needs coverage.
---


## Scope

Limit this skill to focused backend tests and safe verification.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: changed behavior; Application.Tests project; neighboring tests; applicable execution boundaries.

## Architecture

Preserve Api -> Infrastructure -> Application -> Domain and keep transport, use-case, domain, persistence, and external-adapter concerns in their owning layers.

## Implementation rules

Implement the smallest requested behavior, propagate cancellation across async boundaries, preserve unrelated changes, and add focused tests when behavior changes.

## Project conventions

Match neighboring namespaces, file placement, type shapes, registration style, and verified commands; do not add dependencies or a parallel framework without approval.

## Decision rules

Ask before an architecture change, production dependency, breaking contract, migration artifact, database/external connection, or materially broader behavior.

## Anti-patterns

Do not bypass layer boundaries, copy questionable precedent blindly, expose secrets, edit generated output, or start the development API for routine verification.

## Related skills

Compose with: all backend implementation skills.

## Repository references

Start with: changed behavior; Application.Tests project; neighboring tests; applicable execution boundaries.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

xUnit 2.9.2; Microsoft.NET.Test.Sdk 17.12.0; net9.0 test project; net10.0 backend.

**Technical validation:** 2026-07.
# Backend Testing

Test the behavior at the lowest layer that can prove it, then add boundary tests only for risks not covered below.

## Choose the test surface

- Domain entity/value-object tests: invariants, normalization, equality, state transitions, derived values, and failed-mutation atomicity.
- Validator tests: boundaries, conditional rules, collections, and async checks.
- Handler tests: orchestration, port calls, result/exception behavior, and cancellation.
- Mapping/query tests: mapping configuration, EF translation, filters, ordering, paging, and counts.
- API tests: binding, status codes, contracts, authentication, authorization, and exception mapping.
- Adapter tests: SDK request construction, disposal, retries, and failure translation without real external calls by default.

## Workflow

1. Inspect the changed behavior, existing test project/package versions, and neighboring tests.
2. Write a regression test first when fixing a reproducible defect.
3. Use Arrange-Act-Assert and behavior-oriented test names.
4. Assert observable state/results and important port interactions; avoid tests coupled to private implementation.
5. Cover a normal case, boundary/invalid case, and the material failure path.
6. Use deterministic time, identifiers, culture, and synthetic non-secret data.
7. Reuse existing test packages. Ask before adding a mock, container, database, or other dependency.
8. Run the smallest relevant test command and report skipped/blocked checks.

## Repository constraints

Application.Tests currently targets net9.0 while Domain targets net10.0. Treat this as a known target-framework mismatch: report it if it blocks test execution and do not silently retarget either project.

The current suite uses xUnit and direct assertions. Prefer hand-written fakes for small Application ports unless an approved mocking package already exists. Do not start the API, SQL Server, Docker, Azurite, SMTP, or device integrations for routine verification.

## Data and security

Do not use production-like personal data, credentials, tokens, connection strings, or external accounts. Database/infrastructure integration tests require explicit approval before connecting or mutating state. Clean up only resources created by the test and only within the approved target.

## Verify

Use the exact command evidenced by the test project/solution and ensure the process exits rather than watches. Report passed tests, failures, and framework/configuration blockers separately.

## Version handling

Validated in 2026-07 against xUnit 2.9.2, Microsoft.NET.Test.Sdk 17.12.0, a net9.0 test project, and net10.0 backend projects. Revalidate runner/SDK compatibility after framework or test-package upgrades.
