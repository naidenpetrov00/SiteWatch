# Angular Dashboard Architecture

Use a simple feature-based Angular structure. Do not use Nx or `packages/` for now.

## Structure

```text
src/app/
├── core/                  # app-wide infrastructure
│   ├── services/
│   ├── interceptors/
│   └── guards/
│
├── layout/                # dashboard shell
│   ├── sidebar/
│   ├── topbar/
│   └── admin-layout/
│
├── features/              # business features
│   └── invoice-reading/
│       ├── pages/         # routed screens
│       ├── components/    # feature-only components
│       ├── services/      # feature API/services/state
│       ├── models/        # feature interfaces/types
│       └── utils/         # feature helpers
│
├── shared/                # reusable code used by multiple features
│   ├── components/
│   ├── pipes/
│   ├── directives/
│   ├── models/
│   └── utils/
│
├── app.routes.ts
├── app.config.ts
└── app.component.ts
## Placement Rules

When creating or moving files, use these rules:

- App-wide service, auth, API client, interceptor, guard -> `src/app/core/`
- Sidebar, topbar, navigation, shell layout -> `src/app/layout/`
- Business feature code -> `src/app/features/<feature>/`
- Routed page/screen -> `src/app/features/<feature>/pages/`
- Component used only by one feature -> `src/app/features/<feature>/components/`
- Feature API call, service, state, facade/store -> `src/app/features/<feature>/services/`
- Feature interface, DTO, enum, type -> `src/app/features/<feature>/models/`
- Feature helper, formatter, mapper, validator -> `src/app/features/<feature>/utils/`
- Component reused by multiple features -> `src/app/shared/components/`
- Model/type reused by multiple features -> `src/app/shared/models/`
- Helper reused by multiple features -> `src/app/shared/utils/`
- Pipe reused by multiple features -> `src/app/shared/pipes/`
- Directive reused by multiple features -> `src/app/shared/directives/`

## Dependency Rules

Allowed:

- features may import from core
- features may import from shared
- layout may import from core
- layout may import from shared
- core may import from shared

Avoid:

- shared importing from features
- core importing from features
- one feature importing directly from another feature

If multiple features need the same code, move it to shared.

## Assistant Rules

When asked where a file should go, answer using this architecture.

For UI work, prefer `@angular/material` components where they fit the requirement, unless the user explicitly asks for another UI library or component type.

Before creating a file, decide what it is:

- app-wide infrastructure -> core
- layout/navigation -> layout
- routed feature page -> features/<feature>/pages
- feature-only component -> features/<feature>/components
- feature service/API/state -> features/<feature>/services
- feature model/type -> features/<feature>/models
- feature helper -> features/<feature>/utils
- reusable cross-feature code -> shared

Keep it simple. Do not create Nx-style `packages/` or extra libraries unless explicitly requested.

## Component Generation Rule

- When generating a component, keep the template and CSS in separate files.
- Do not use inline `template` or `styles` in `@Component` unless explicitly requested.

## Test Rule

- Do not write tests unless the user explicitly asks for them.
