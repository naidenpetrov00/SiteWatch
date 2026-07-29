---
name: mediator-use-cases
description: Implement Application-layer commands, queries, and handlers with MediatR while preserving clean dependency boundaries. Use when adding or changing IRequest records, IRequestHandler implementations, orchestration, cancellation, or use-case folder structure.
---


## Scope

Limit this skill to Application commands, queries, handlers, and orchestration.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: the feature request/handler; neighboring use case; required ports; relevant tests.

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

Compose with: validation-pipeline; application-ports; ef-queries; ef-writes-transactions; dto-mapping.

## Repository references

Start with: the feature request/handler; neighboring use case; required ports; relevant tests.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET 10; MediatR 14.2.0.

**Technical validation:** 2026-07.
# Mediator Use Cases

Model each application action as a focused request and keep transport concerns out of its handler.

## Workflow

1. Inspect a neighboring feature under src/Application and the ports it uses.
2. Choose a command for state change and a query for read-only retrieval.
3. Define a sealed request record with the smallest complete input and a deliberate response type.
4. Put the handler beside the request unless the established feature separates it.
5. Inject Application interfaces, IApplicationDbContext for supported data access, IMapper where projection is justified, and no Api types.
6. Orchestrate domain behavior and ports; do not duplicate invariants already owned by entities or value objects.
7. Propagate the supplied CancellationToken to every cancellable async operation.
8. Add a validator for untrusted or cross-resource input and focused tests for behavior.

## Handler rules

- Keep one clear use case per request.
- Prefer direct return types for successful results and established Result models for expected Identity-style failures.
- Use the established not-found exception pattern when a required resource does not exist.
- Do not catch exceptions merely to rethrow; centralized pipeline behavior owns cross-cutting handling.
- Do not call another endpoint or bind HttpContext in Application.
- Avoid one-line pass-through handlers that add no policy only when the port boundary is still useful; do not collapse layers incidentally.
- Treat multi-system workflows as explicit orchestration with compensation, not as one database transaction.

## Composition

Use validation-pipeline for validators, api-authorization for request authorization, ef-queries or ef-writes-transactions for data access, dto-mapping for outputs, and application-ports for external dependencies.

## Verify

Build src/Application/Application.csproj and the smallest relevant tests. Check request discovery through AddMediatR, cancellation propagation, dependency direction, validation activation, and absence of transport/infrastructure types.

## Version handling

Validated in 2026-07 against MediatR 14.2.0 and .NET 10. Request/handler architecture is stable; use Context7 if registration, pipeline, notification, or handler signatures differ after a MediatR upgrade.
