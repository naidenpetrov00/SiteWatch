---
name: validation-pipeline
description: Add and maintain FluentValidation validators and the MediatR validation behavior for Application requests. Use for request rules, asynchronous existence checks, collection validation, conditional validation, validation error behavior, or validator registration.
---


## Scope

Limit this skill to FluentValidation request validation through MediatR.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: the request; related validator/base validator; domain invariants; persistence constraints.

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

Compose with: mediator-use-cases; domain-entities; api-request-pipeline.

## Repository references

Start with: the request; related validator/base validator; domain invariants; persistence constraints.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

FluentValidation 12.1.1; MediatR 14.2.0; EF Core 10.0.10.

**Technical validation:** 2026-07.
# Validation Pipeline

Validate request shape and application preconditions before the handler runs; keep durable domain invariants in Domain.

## Place each rule

- Put required fields, formats, lengths, ranges, enum validity, mutually exclusive inputs, and collection shape in a request validator.
- Put database-backed existence or uniqueness checks in async validator rules only when early validation is valuable and race conditions are still handled by the write path.
- Put business invariants that must hold for every caller in entities or value objects.
- Put authorization in api-authorization, not in a validation rule.

## Workflow

1. Inspect the request, its base DTO/validator, domain behavior, and corresponding persistence constraints.
2. Create an AbstractValidator<TRequest> beside the request using RuleFor, When, RuleForEach, and CascadeMode.Stop deliberately.
3. Reuse shared validators/rule helpers only for real repeated semantics.
4. Pass CancellationToken into all MustAsync queries and use AsNoTracking for read-only checks.
5. Produce stable, field-addressable messages; do not reveal sensitive records or credentials.
6. Confirm AddValidatorsFromAssembly discovers the validator and ValidationBehaviour remains registered.
7. Test valid, boundary, invalid, conditional, and collection cases.

## Avoid

- Duplicating every domain guard in validators without considering different trust boundaries.
- Assuming an asynchronous uniqueness check prevents a race; enforce uniqueness in the database too.
- Calling external email, blob, or device systems from validation.
- Dereferencing nullable values after a failed rule without CascadeMode.Stop or a condition.
- Adding manual endpoint validation when the MediatR behavior already covers the request.
- Changing the global validation error contract incidentally.

## Verify

Build src/Application/Application.csproj and run focused validator/use-case tests. Confirm the handler is not called on failure, asynchronous rules observe cancellation, and the API middleware maps ValidationException consistently.

## Version handling

Validated in 2026-07 against FluentValidation 12.1.1, its dependency-injection extensions, and MediatR 14.2.0. Consult Context7 for changed cascade, async validation, scanning, or pipeline behavior after upgrades.
