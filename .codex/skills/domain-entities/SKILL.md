---
name: domain-entities
description: Create and evolve domain entities, aggregate behavior, invariants, child collections, audit inheritance, and domain events under src/Domain. Use when adding entity factories, state transitions, invariant enforcement, relationships, or aggregate behavior.
---


## Scope

Limit this skill to domain entities, invariants, aggregate behavior, collections, and events.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: Domain seed work; target entity/value objects; EF configuration; focused tests.

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

Compose with: value-objects; ef-modeling; ef-writes-transactions; backend-testing.

## Repository references

Start with: Domain seed work; target entity/value objects; EF configuration; focused tests.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET 10; Ardalis.GuardClauses 5.0.0; MediatR 14.2.0.

**Technical validation:** 2026-07.
# Domain Entities

Make the entity the authoritative home for invariants that must hold regardless of caller.

## Workflow

1. Inspect BaseEntity, BaseAuditableEntity, seed-work interfaces, related value objects, EF configuration, and focused domain tests.
2. Identify the aggregate boundary, lifecycle, valid states, invariants, derived values, and owned child collections.
3. Use a private parameterless constructor only for persistence and a factory or valid constructor for creation.
4. Keep setters private where practical and expose intention-revealing methods for state changes.
5. Validate and normalize all inputs before assigning any fields so a failed operation does not partially mutate the entity.
6. Encapsulate mutable collections behind IReadOnlyCollection and controlled add/remove/replace methods.
7. Generate identifiers according to the established aggregate pattern; preserve database-generated NumberId behavior.
8. Add focused tests for creation, transitions, boundaries, derived values, invalid operations, and mutation atomicity.

## Rules

- Keep HTTP, EF configuration, storage, email, and other infrastructure behavior out of entities.
- Reuse value-objects for cohesive concepts with equality and normalization.
- Use Ardalis guards where they express the rule clearly; use explicit checks for cross-field or collection invariants.
- Keep derived/search fields synchronized inside the same successful state transition.
- Preserve relationship consistency on both sides when the aggregate owns it.
- Do not make public setters merely for EF; use supported constructors, backing fields, and configuration.
- Avoid generic repository abstractions unless a concrete use case requires one.

## Domain events

BaseEntity already stores BaseEvent notifications and ApplicationDbContext dispatches them before SaveChanges. Add a concrete domain event only when another in-process reaction is genuinely required. Compose with ef-writes-transactions and account for the current pre-save dispatch timing; do not assume it provides outbox or post-commit guarantees.

## Verify

Run focused Domain tests and build src/Domain/Domain.csproj. Check invalid changes leave prior state intact, equality/collections behave as intended, and persistence configuration can materialize the entity.

## Version handling

Validated in 2026-07 against .NET 10, Ardalis.GuardClauses 5.0.0, and the current domain seed work. Domain rules are version-agnostic; consult Context7 only when framework persistence or MediatR event mechanics affect the implementation.
