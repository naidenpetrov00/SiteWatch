---
name: api-authorization
description: Secure backend endpoints and MediatR requests with JWT bearer authentication, endpoint authorization, roles, claims, or policies. Use when changing RequireAuthorization, public routes, authorization behaviors, claims, policies, or current-user access.
---


## Scope

Limit this skill to backend authentication and authorization enforcement.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: endpoint groups; JWT registration; authorization behavior; Identity ports/adapters.

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

Compose with: identity-integration; minimal-api-endpoints; api-request-pipeline.

## Repository references

Start with: endpoint groups; JWT registration; authorization behavior; Identity ports/adapters.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

ASP.NET Core Authentication/Authorization 10.0.10; ASP.NET Core Identity 10.0.10; System.IdentityModel.Tokens.Jwt 8.19.2.

**Technical validation:** 2026-07.
# API Authorization

Apply authentication at the HTTP boundary and authorization at the layer that owns the decision.

## Workflow

1. Identify the caller, resource, action, and required role, claim, policy, or ownership rule.
2. Inspect endpoint grouping, JWT bearer configuration, AuthorizeAttribute, AuthorizationBehaviour, IUser, and Identity adapters.
3. Require authentication with RequireAuthorization on the narrowest coherent route group.
4. Put reusable use-case authorization on the MediatR request when authorization depends on application policy.
5. Put resource ownership checks in the use case/query where the resource is loaded; do not rely only on client-supplied IDs.
6. Return or map unauthenticated access to 401 and authenticated-but-forbidden access to 403.
7. Add focused tests for anonymous, allowed, and denied cases.

## Guardrails

- Default new sensitive endpoints to protected unless requirements explicitly make them public.
- Never authorize from unverified request fields, email strings, or UI state.
- Do not log bearer tokens, password/reset tokens, or sensitive claims.
- Keep authentication scheme and token validation consistent between token creation and AddJwtBearer.
- Do not activate the currently unregistered authorization pipeline behavior incidentally; registration changes affect every MediatR request and require cross-feature review.
- Treat claim names, roles, policies, issuer, audience, signing algorithm, and lifetime as security-sensitive contracts.

## Compose with

Use identity-integration for Identity/JWT implementation, minimal-api-endpoints for route enforcement, and api-request-pipeline for 401/403 exception mapping.

## Verify

Build affected projects. Inspect middleware order, endpoint metadata, request attributes, and resource checks. Do not start the API. For current security behavior or newer framework APIs, validate with Context7 before changing defaults.

## Version handling

Validated in 2026-07 against ASP.NET Core Authentication/Authorization 10.0.10, Identity 10.0.10, and System.IdentityModel.Tokens.Jwt 8.19.2. Revalidate affected guidance after security advisories or version upgrades even if architecture is unchanged.
