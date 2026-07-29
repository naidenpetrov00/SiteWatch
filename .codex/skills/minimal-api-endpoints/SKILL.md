---
name: minimal-api-endpoints
description: Create or modify ASP.NET Core minimal API endpoint groups that bind HTTP input, send MediatR requests, and return typed HTTP results. Use for route mapping, handler signatures, status codes, route groups, OpenAPI metadata, or endpoint registration under src/Api.
---


## Scope

Limit this skill to minimal API route groups and handlers.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: src/Api/Program.cs; src/Api/SeedWork/EndpointGroupBase.cs; src/Api/SeedWork/Extensions/WebApplicationExtension.cs; a neighboring endpoint.

## Architecture

Preserve Api -> Infrastructure -> Application -> Domain and keep transport, use-case, domain, persistence, and external-adapter concerns in their owning layers.

## Implementation rules

Implement the smallest requested behavior, propagate cancellation across async boundaries, preserve unrelated changes, and add focused tests when behavior changes.

## Project conventions

Match neighboring namespaces, file placement, type shapes, registration style, and verified commands; do not add dependencies or a parallel framework without approval.

## Anti-patterns

Do not bypass layer boundaries, copy questionable precedent blindly, expose secrets, edit generated output, or start the development API for routine verification.

## Related skills

Compose with: http-contracts; api-authorization; api-request-pipeline; file-uploads; mediator-use-cases.

## Repository references

Start with: src/Api/Program.cs; src/Api/SeedWork/EndpointGroupBase.cs; src/Api/SeedWork/Extensions/WebApplicationExtension.cs; a neighboring endpoint.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

ASP.NET Core 10.0.10; MediatR 14.2.0.

**Technical validation:** 2026-07.
# Minimal API Endpoints

Keep endpoint handlers thin and preserve the repository's reflection-discovered EndpointGroupBase pattern.

## Workflow

1. Inspect src/Api/Program.cs, SeedWork/EndpointGroupBase.cs, SeedWork/Extensions/WebApplicationExtension.cs, and a nearby endpoint group.
2. Add or change a public EndpointGroupBase subclass under src/Api/Endpoints.
3. Map routes in Map(WebApplication app) with MapGroupCustom; preserve existing route families and constraints.
4. Bind route, query, JSON, or form input explicitly. Use [AsParameters] for established table-query binding and [FromForm] for mixed form fields.
5. Construct or bind an Application command/query, call IMediator.Send, and return TypedResults.
6. Forward CancellationToken from the handler through MediatR for new or changed async flows.
7. Apply group- or endpoint-level authorization deliberately; do not infer public access from an existing unsecured endpoint.
8. Add OpenAPI metadata when it improves the public contract and matches current project practice.

## HTTP semantics

- GET single: return 200; let centralized not-found mapping handle missing resources when the use case follows that pattern.
- GET collection: return 200 with a DTO collection or PagedResult<T>.
- POST create: prefer 201 Created with a correct resource URI and identifier response.
- PUT: use for full replacement semantics; PATCH for partial updates or actions.
- DELETE: return 204 after successful deletion.
- Use explicit Results<T1,...> when multiple typed outcomes are returned; use IResult only when branches make typed unions impractical.
- Do not return domain entities or EF entities.

## Decision rules

- Keep API transport logic in Api; validation and use-case orchestration belong in Application.
- Use built-in OpenAPI already registered by AddOpenApi and MapOpenApi; do not add Swagger packages.
- Endpoint discovery is reflection-based. A group that does not inherit EndpointGroupBase will not be mapped.
- Do not start the API to verify endpoints because development startup mutates the database.

## Compose with

Use http-contracts for request/response design, api-authorization for access rules, file-uploads for multipart endpoints, and api-request-pipeline for centralized errors. Use mediator-use-cases for the request itself.

## Verify

Build src/Api/Api.csproj without starting it. Confirm route uniqueness, route constraints, binding sources, status codes, created-location paths, cancellation propagation, authorization, and OpenAPI-visible result types.

## Version handling

Adapted from the installed dotnet-webapi skill and validated in 2026-07 for ASP.NET Core 10. Preserve stable HTTP rules across versions; query Context7 when minimal API binding, typed results, OpenAPI, or endpoint-filter behavior differs in a newer installed ASP.NET Core version.
