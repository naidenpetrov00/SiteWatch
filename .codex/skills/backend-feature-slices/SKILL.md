---
name: backend-feature-slices
description: Implement complete backend features across Domain, Application, Infrastructure, and Api while preserving dependency direction. Use when a request adds or changes a use case that spans multiple backend layers, especially CRUD, workflow, or integration-backed features.
---


## Scope

Limit this skill to complete cross-layer backend feature orchestration.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: AGENTS.md; backend project files; one representative feature and its tests.

## Architecture

Preserve Api -> Infrastructure -> Application -> Domain and keep transport, use-case, domain, persistence, and external-adapter concerns in their owning layers.

## Workflow

Apply the ordered capability workflow above after inspecting one representative implementation.

## Implementation rules

Implement the smallest requested behavior, propagate cancellation across async boundaries, preserve unrelated changes, and add focused tests when behavior changes.

## Project conventions

Match neighboring namespaces, file placement, type shapes, registration style, and verified commands; do not add dependencies or a parallel framework without approval.

## Decision rules

Ask before an architecture change, production dependency, breaking contract, migration artifact, database/external connection, or materially broader behavior.

## Anti-patterns

Do not bypass layer boundaries, copy questionable precedent blindly, expose secrets, edit generated output, or start the development API for routine verification.

## Related skills

Compose with: domain-entities; mediator-use-cases; application-ports; ef-modeling; minimal-api-endpoints; backend-testing.

## Repository references

Start with: AGENTS.md; backend project files; one representative feature and its tests.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

.NET 10; ASP.NET Core 10.0.10; EF Core 10.0.10; MediatR 14.2.0; FluentValidation 12.1.1; AutoMapper 16.2.0.

**Technical validation:** 2026-07.
# Backend Feature Slices

Build the smallest complete vertical slice that satisfies the requested behavior. Compose the focused skills for each capability instead of duplicating their detailed rules here.

## Establish the slice

1. Read the root and nearest AGENTS.md files, project manifests, related tests, and one representative feature.
2. Trace the dependency direction Api -> Infrastructure -> Application -> Domain.
3. List the behavior, authorization, validation, persistence, integration, contract, and test impacts before editing.
4. Reuse an existing feature folder and naming pattern. Do not generate controllers, Razor UI, or direct endpoint-to-DbContext access.
5. Ask before adding dependencies, changing architecture, generating migrations, or expanding the requested behavior.

## Place responsibilities

- Put invariants and state transitions in Domain; keep transport and infrastructure dependencies out.
- Put commands, queries, validators, DTOs, mapping, and required ports in Application.
- Put EF configuration and external-service adapters in Infrastructure.
- Put route binding, HTTP semantics, authentication wiring, and middleware in Api.
- Keep camera, invoice, or future vendor behavior behind ordinary application ports and infrastructure adapters; do not create project-specific integration patterns without a distinct need.

## Implement in dependency order

1. Model or revise domain behavior with domain-entities and value-objects.
2. Define application ports and use cases with application-ports, mediator-use-cases, and validation-pipeline.
3. Add persistence with ef-modeling, ef-queries, or ef-writes-transactions; use ef-schema-changes when the model changes.
4. Implement external adapters with the relevant integration skill.
5. Register services with backend-composition and options-configuration.
6. Expose the behavior with minimal-api-endpoints, http-contracts, and api-authorization.
7. Add focused coverage with backend-testing.

## Preserve repository boundaries

- Do not introduce references from inner projects to outer projects.
- Do not let endpoints orchestrate domain or persistence work; send an application request through MediatR.
- Do not add a second framework or parallel architectural style when an established path exists.
- Treat existing code as evidence, not automatic permission to reproduce questionable or insecure patterns.
- Do not start the API for verification: development startup applies migrations and seeds data.
- Do not create or edit migrations unless explicitly requested.

## Verify

Review every affected layer, then run the smallest evidenced build and existing-test commands. Confirm cancellation flows across async boundaries, contracts do not expose entities, authorization is enforced at the correct boundary, and no unrelated files changed.

## Version handling

Stable architecture rules are version-independent. This skill was validated in 2026-07 against .NET 10 and the repository's current backend shape. If detected framework versions are newer or a framework behavior is uncertain, consult current Context7 documentation only for the affected capability.
