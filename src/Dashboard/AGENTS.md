# Dashboard instructions

## Scope and ownership

- This file applies to `src/Dashboard`.
- Dashboard is an Angular 21 application. Preserve its standalone application setup, lazy route ownership, feature-oriented structure, core authentication/API concerns, and TanStack Angular Query boundary unless the request calls for an architectural change.
- Keep feature-specific pages, components, services, and models within the owning feature. Put genuinely application-wide behavior under the existing core or application-level setup.

## Skill routing

- Use the `angular-developer` skill for Angular implementation and architecture guidance.
- Use `web-design-guidelines` when the user requests a UI, accessibility, or interface-guideline review.
- Load additional skills only when the task actually requires them. Keep concrete Angular syntax and procedures in skills, not this AGENTS file.

## Commands and generated content

- `package.json` and `angular.json` are the authoritative command/configuration sources. The defined scripts are `start`, `build`, `watch`, and `test`; execution remains governed by the root authorization policy.
- Do not directly edit `dist`, `.angular`, `node_modules`, coverage output, or other generated artifacts.
