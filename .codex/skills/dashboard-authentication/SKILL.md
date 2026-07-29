---
name: dashboard-authentication
description: Implement and secure Dashboard client authentication behavior, including JWT session state, sessionStorage, functional HTTP interception, public-request bypass, login and logout, token-expiry UX, and authenticated or guest route guards. Use for src/app/core/auth, the identity feature, authorization headers, 401 or 403 handling, or authenticated routing changes.
---

# Dashboard Authentication

Preserve the existing session architecture while treating the API as the security boundary.

## Scope and required context: inspect both sides

Read core/auth, the identity service and pages, guards, app.config.ts, app.routes.ts, and their tests. Inspect the relevant API authentication and authorization contracts read-only. For every security-sensitive framework decision, validate current guidance with Context7 even when local precedent exists.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: session management

- Keep access-token state centralized in AuthSessionService.
- Preserve sessionStorage as the approved current persistence mechanism unless the user approves an authentication architecture change.
- Guard all browser storage access so code remains safe during tests and possible server rendering.
- Reject and remove an expired or unusable stored token during restoration.
- Expose readable session signals and keep storage mutation inside the service.
- Clear the token and derived logged-in state together.
- Never log, display, commit, place in URLs, or copy access tokens.

## Send authenticated requests

- Use the functional auth interceptor registered through provideHttpClient.
- Add Authorization: Bearer only when a token exists.
- Preserve an HttpContext token for explicitly public requests such as sign-in.
- Do not bypass authentication based on string URL matching.
- Clear the session on a response that proves credentials are invalid, normally 401.
- Do not automatically treat every 403 as an invalid session; 403 can mean an authenticated user lacks permission. Confirm the backend contract before clearing state.
- Re-throw HTTP errors so the query or caller can represent failure.

## Implement login and routing

- Map backend identity envelopes through focused utilities.
- Persist a token only after a successful mapped sign-in result.
- Clear stale session state on failed authentication.
- Return safe user-facing errors without exposing raw payloads.
- Use functional guest and authenticated guards for navigation UX.
- Return UrlTree redirects from guards.
- Never describe a route guard as authorization enforcement; endpoints must enforce access.
- On logout, clear the session before navigating to sign-in.

## Related skills and repository references

- Use dashboard-data-access for HttpClient and mutations.
- Use dashboard-routing-composition for guard placement.
- Use dashboard-async-ui-state for sign-in and expiry feedback.
- Use dashboard-ssr-hydration for browser-only session behavior during SSR.
- Use dashboard-testing for interceptor, storage, guard, and identity-service tests.

## Verification and definition of done

Test valid restore, expired restore, login success and failure, public-request bypass, bearer attachment, 401 behavior, 403 policy, logout, and both guard outcomes. Run npm run build and relevant npm test checks.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular HTTP and Router 21.2.13 and the current SiteWatch JWT contract. Treat client storage as exposed to same-origin script and keep XSS prevention in scope. Use Context7 for any changed Angular security API or unfamiliar authentication flow.
