---
name: identity-integration
description: Implement and secure ASP.NET Core Identity and JWT bearer workflows including signup, signin, verification, reset, users, roles, claims, token creation, and token validation. Use for Identity services, ApplicationUser, authentication endpoints, JWT options, or authorization integration.
---


## Scope

Limit this skill to ASP.NET Core Identity and JWT workflows.

## Required context

Read the applicable AGENTS.md, authoritative manifests, and these task-relevant sources: Identity registration/services; JwtTokenService; JWT options/bearer validation; identity endpoints/commands.

## Implementation rules

Implement the smallest requested behavior, propagate cancellation across async boundaries, preserve unrelated changes, and add focused tests when behavior changes.

## Project conventions

Match neighboring namespaces, file placement, type shapes, registration style, and verified commands; do not add dependencies or a parallel framework without approval.

## Decision rules

Ask before an architecture change, production dependency, breaking contract, migration artifact, database/external connection, or materially broader behavior.

## Anti-patterns

Do not bypass layer boundaries, copy questionable precedent blindly, expose secrets, edit generated output, or start the development API for routine verification.

## Related skills

Compose with: api-authorization; api-request-pipeline; options-configuration; email-delivery.

## Repository references

Start with: Identity registration/services; JwtTokenService; JWT options/bearer validation; identity endpoints/commands.

## Verification

Complete the focused checks described above, use only repository-evidenced commands, and report blocked checks separately.

## Definition of done

Finish when the requested behavior is correctly owned, boundaries and contracts are preserved, relevant tests/checks pass, and no unauthorized side effects or unrelated changes remain.

## Context7 fallback

Use Context7 narrowly when the installed version is newer than this validation, an API is uncovered or uncertain, repository evidence conflicts with current framework behavior, or security/performance guidance requires revalidation.

## Validated technologies

ASP.NET Core Identity/JwtBearer 10.0.10; System.IdentityModel.Tokens.Jwt 8.19.2.

**Technical validation:** 2026-07.
# Identity Integration

Treat identity changes as security-sensitive and validate current framework guidance before changing cryptography, tokens, cookies, roles, claims, or password behavior.

## Architecture

- Keep Identity entities and provider implementations in Domain/Infrastructure only where the current architecture already requires them; do not spread Identity APIs further inward.
- Expose use-case-oriented methods through Application's IIdentityService.
- Keep endpoint binding/results in Api and use MediatR commands/queries for workflows.
- Keep JWT validation wiring in Api and token generation in the Infrastructure identity service.

## Workflow

1. Define the threat-sensitive workflow, caller, expected state transitions, enumeration risk, token lifetime, and failure contract.
2. Inspect AddIdentity, AddJwtBearer, the split Identity services, JwtTokenService, option binding, email delivery, and current claims.
3. Use UserManager, SignInManager, RoleManager, and token providers instead of reimplementing password hashing or reset/verification tokens.
4. Normalize public failure responses to avoid account enumeration where appropriate.
5. Generate JWTs with a unique ID, stable subject/user ID, required claims, UTC-based expiry, configured issuer/audience, and an approved signing algorithm.
6. Validate signature, issuer, audience, and lifetime consistently; keep clock-skew and expiry decisions explicit.
7. Rotate or revoke credentials/tokens according to requirements and invalidate relevant security state after sensitive changes.
8. Add tests for success, invalid credentials/token, expired token, unverified account, role/claim checks, and enumeration-safe behavior.

## Guardrails

- Never log passwords, JWTs, verification/reset tokens, signing keys, or credential-bearing user data.
- Never hard-code token lifetimes or secrets for a production path; consume validated options.
- Do not return raw Identity errors unless their disclosure is explicitly safe.
- Use purpose-bound, single-use framework tokens for email verification and password reset.
- Enforce HTTPS outside controlled local development.
- Treat administrator assignment and policy changes as privileged operations requiring authorization and audit review.

## Compose with

Use api-authorization, api-request-pipeline, options-configuration, email-delivery, and backend-testing.

## Verify

Build Api, Application, Domain, and Infrastructure as affected. Do not start the API or send email. Review package advisories/current Context7 guidance and test token validation with synthetic keys only.

## Version handling

Validated in 2026-07 against ASP.NET Core Identity/JwtBearer 10.0.10 and System.IdentityModel.Tokens.Jwt 8.19.2. Revalidate on every security advisory or upgrade, even when public APIs appear unchanged.
