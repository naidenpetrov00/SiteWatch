---
name: ef-writes-transactions
description: Implement EF Core create, update, delete, transaction, concurrency, and domain-event write workflows. Use when changing tracked aggregates, using ExecuteUpdate/Delete, coordinating multiple SaveChanges calls, or reasoning about atomicity and partial failure.
---


## Scope

Limit this skill to EF Core writes, transactions, concurrency, and domain-event timing.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: use case; entity behavior; DbContext save override; relevant adapters and tests.

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

Compose with: domain-entities; ef-modeling; application-ports; backend-testing.

## Repository references

Start with: use case; entity behavior; DbContext save override; relevant adapters and tests.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

EF Core SQL Server 10.0.10; MediatR 14.2.0.

**Technical validation:** 2026-07.
# EF Writes and Transactions

Use the narrowest transaction boundary that preserves the requested invariant.

## Workflow

1. Identify every state change and external side effect in the use case.
2. Load tracked entities only when domain behavior or relationship fix-up is required.
3. Apply changes through domain methods, add/remove through the DbContext, and call SaveChangesAsync once when one unit of work suffices.
4. Use an explicit transaction only when multiple saves or set-based operations must be atomic.
5. Pass CancellationToken to queries, SaveChangesAsync, transaction creation, commit, rollback, and external calls.
6. Handle not-found, uniqueness, concurrency, and retry semantics explicitly.
7. Test success, validation failure, missing resources, and rollback/partial-failure paths.

## Repository-specific behavior

ApplicationDbContext dispatches tracked BaseEntity events before base SaveChangesAsync. Handlers therefore run before the database commit and do not provide an outbox guarantee. Do not add side-effecting events without analyzing failure and retry behavior.

Set-based ExecuteUpdateAsync and ExecuteDeleteAsync bypass entity methods, change tracking, audit hooks, and domain events. Use them only when those semantics are intentionally unnecessary, then check affected-row counts where absence matters.

## Transaction rules

- Do not wrap every single SaveChanges in an explicit transaction; EF already makes one save atomic when supported.
- Keep transactions short and never wait on email, blob storage, device calls, or user interaction while holding a database transaction.
- A database transaction cannot roll back external storage or email. Design ordering, idempotency, or compensation.
- Do not swallow DbUpdateConcurrencyException or DbUpdateException; translate only when the application has a defined outcome.
- Preserve SQL Server execution-strategy requirements when combining retries and manual transactions.

## Compose with

Use domain-entities for state changes, ef-modeling for constraints/concurrency tokens, application-ports for external effects, and backend-testing for rollback behavior.

## Verify

Build affected backend projects and run focused tests. Never connect to or mutate a database without explicit approval. Inspect that every path commits once or fails without reporting false success.

## Version handling

Validated in 2026-07 against EF Core SQL Server 10.0.10 and MediatR 14.2.0. Consult Context7 for current transaction, execution-strategy, concurrency, SaveChanges interception, and bulk-operation behavior after upgrades.
