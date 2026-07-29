---
name: dashboard-routing-composition
description: Configure the Angular Dashboard bootstrap, application providers, lazy routes, nested shells, navigation, route data, resolvers, and functional guards. Use when changing app.config.ts, app.routes.ts, RouterLink or Router behavior, route-scoped providers, titles, redirects, route inputs, or authenticated route composition.
---

# Dashboard Routing and Composition

Keep bootstrap and navigation explicit, lazy, and compatible with authentication and future rendering modes.

## Scope and required context: inspect composition roots

Read app.config.ts, app.routes.ts, the dashboard shell, identity guards, and any affected routed page. Confirm package versions before using version-sensitive router features.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: providers

- Keep application singletons and cross-cutting providers in app.config.ts.
- Preserve provideHttpClient with functional interceptors, provideTanStackQuery, provideRouter, and withComponentInputBinding unless the task changes their ownership.
- Scope feature-only services at a route or component only when their lifetime must match that scope.
- Use providedIn: 'root' for established stateless or application-wide services; use @Injectable, never a fabricated service decorator.
- Do not add providers to solve import or ownership problems.

## Define routes

- Lazy-load routed standalone components with loadComponent.
- Use a shell route with children for authenticated layout and navigation.
- Order routes by first-match semantics and put any wildcard last.
- Give user-visible pages meaningful route titles.
- Prefer route parameters or query parameters for shareable navigation state.
- Use component input binding for route parameters, query parameters, static data, and resolver results when it keeps the page API explicit.
- Use resolvers only for data that must exist before activation; otherwise let the page show normal async UI state.

## Implement guards and navigation

- Use functional CanMatch, CanActivate, or CanDeactivate guards.
- Return a UrlTree or RedirectCommand for redirects rather than imperatively navigating inside a guard.
- Treat client guards as navigation behavior only. The API must enforce authorization independently.
- Use RouterLink for declarative navigation and Router.navigate or navigateByUrl for workflow completion.
- Preserve nested RouterOutlet ownership in the application root and dashboard shell.
- Test active navigation states rather than duplicating path logic across components when RouterLinkActive can express the rule.

## Related skills and repository references

- Use dashboard-authentication for session and guard policy.
- Use dashboard-async-ui-state for resolver or navigation feedback.
- Use dashboard-ssr-hydration before changing rendering strategy.
- Use dashboard-testing for RouterTestingHarness and guard tests.

## Verification and definition of done

- Test allowed, redirected, unknown, and nested-route cases.
- Confirm lazy imports resolve and route titles remain correct.
- Run npm run build and relevant npm test checks.
- Do not start npm start or npm run watch unless explicitly requested.

## Context7 fallback and validated technologies

Validated in 2026-07 for Angular Router 21.2.13 and the Dashboard standalone route structure. Query Context7 for newer router APIs, hybrid rendering, navigation error handling, or unfamiliar provider scopes.
