---
name: backend-composition
description: Register and compose backend services across Api, Application, and Infrastructure with correct dependency injection lifetimes and startup ordering. Use when changing DependencyInjection.cs files, Program.cs, MediatR/AutoMapper/validator discovery, DbContext, or adapter registrations.
---


## Scope

Limit this skill to backend dependency injection and startup composition.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: Program.cs; all layer DependencyInjection.cs files; interface; implementation; configuration.

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

Compose with: application-ports; options-configuration; api-request-pipeline; identity-integration.

## Repository references

Start with: Program.cs; all layer DependencyInjection.cs files; interface; implementation; configuration.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

Microsoft.Extensions.DependencyInjection 10.0.10; MediatR 14.2.0; AutoMapper 16.2.0; FluentValidation 12.1.1.

**Technical validation:** 2026-07.
# Backend Composition

Keep each layer responsible for registering its own services and compose them only at the Api entry point.

## Registration ownership

- Api registers transport concerns: OpenAPI, CORS, authentication, authorization, and API-only mapping.
- Application registers validators, AutoMapper profiles, MediatR handlers, and pipeline behaviors.
- Infrastructure registers ApplicationDbContext, Identity, provider clients, integration adapters, initializers, and infrastructure MediatR handlers.
- Program.cs calls layer registration methods and configures middleware; it must not become a service-by-service registry.

## Workflow

1. Inspect all three DependencyInjection.cs files, Program.cs, the interface, implementation, and their dependencies.
2. Reuse an existing registration and lifetime pattern.
3. Choose lifetime from actual ownership:
   - scoped for DbContext-dependent request services;
   - singleton only for thread-safe clients/configuration with no scoped dependency;
   - transient for stateless lightweight services when appropriate.
4. Ensure singleton services never capture scoped services.
5. Register interfaces to implementations and concrete types only when framework activation requires them.
6. Verify assembly scanning targets the assembly that contains the handlers, validators, or profiles.
7. Preserve middleware/startup order and environment gates.

## Guardrails

- Do not add a package or scanning framework without approval.
- Avoid duplicate MediatR, AutoMapper, or service registrations unless multiple assemblies are intentional.
- Do not build a temporary ServiceProvider during registration.
- Do not resolve services manually when constructor injection works.
- Do not start the API to validate DI; development startup migrates/seeds the database.
- Treat changing a global pipeline behavior as cross-cutting work requiring representative review.

## Compose with

Use options-configuration for settings, application-ports for boundaries, identity-integration for Identity/JWT, and api-request-pipeline for middleware.

## Verify

Build the smallest affected projects. Inspect registrations for missing/duplicate services and lifetime captivity. Use focused service-provider tests only if they do not start the application or external infrastructure.

## Version handling

Validated in 2026-07 against Microsoft.Extensions.DependencyInjection 10.0.10, MediatR 14.2.0, AutoMapper 16.2.0, and FluentValidation 12.1.1. Consult Context7 after upgrades that affect scanning, keyed services, activation, or lifetime validation.
