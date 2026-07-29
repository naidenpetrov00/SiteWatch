---
name: dashboard-internationalization
description: Assess and implement opt-in Angular internationalization and localization for src/Dashboard, including @angular/localize setup, source and target locales, template and TypeScript messages, extraction, translation files, locale-aware formatting, Material locale behavior, localized builds, deployment paths, tests, and hydration integration. Use when adding languages, locale formatting, translated UI, or i18n build configuration.
---

# Dashboard Internationalization

Design locale ownership and deployment before marking strings.

## Scope and required context: gate adoption

- Confirm the source locale, supported target locales, translation owner, file format, release process, and deployment model.
- Verify package.json and package-lock.json. @angular/localize is currently not installed.
- Ask before adding dependencies or running ng add @angular/localize.
- Select a version compatible with Angular and CLI 21.2.x.
- Do not introduce a runtime translation library unless the user explicitly chooses runtime language switching and approves the dependency.

## Architecture, workflow, implementation conventions, decisions, and anti-patterns: messages

- Use Angular i18n metadata in templates for visible text and translatable attributes.
- Use $localize for user-facing TypeScript messages that cannot live in templates.
- Provide meaningful descriptions where translators need context.
- Use stable custom message IDs only when the translation workflow requires them.
- Keep identifiers, enum wire values, URLs, API field names, and serialized payloads untranslated.
- Treat existing hard-coded English copy as migration inventory, not as a reason for partial untracked translation.

## Manage translations

- Extract source messages with the Angular CLI and keep translation sources under src/Dashboard source control.
- Review additions, removals, duplicate meanings, interpolation placeholders, plurals, and alternate expressions after extraction.
- Do not edit dist output or other generated build artifacts.
- Define sourceLocale and target locales in angular.json.
- Use localized builds deliberately; understand that the CLI can emit locale subdirectories and adjust base href or subPath.
- Coordinate server or CDN Accept-Language redirects with deployment owners because they are external infrastructure changes.

## Localize data and Material

- Configure locale data and LOCALE_ID consistently.
- Use Angular date, number, percent, and currency formatting for user-visible values instead of fixed English formatting.
- Keep API date and number serialization locale-neutral.
- Configure Material date adapters, paginator labels, and component locale behavior where needed.
- Preserve accessible names and translated validation messages.
- Ensure table export headers and user-facing formatted cells follow the approved locale policy.

## Related skills and repository references

- Use dashboard-material-ui for Material and accessibility behavior.
- Use dashboard-models-mapping to keep wire values locale-neutral.
- Use dashboard-ssr-hydration and withI18nSupport when localized blocks hydrate.
- Use dashboard-service-worker for locale-asset caching.
- Use dashboard-testing for extraction-adjacent checks and representative locale behavior.

## Verification and definition of done

- Run message extraction, npm run build, localized build checks supported by approved configuration, and relevant npm test checks.
- Test representative templates, attributes, validation, dates, numbers, currencies, Material controls, routing base paths, missing translations, and fallback locale.
- Do not start servers or change deployment configuration without explicit approval.

## Context7 fallback and validated technologies

Validated in 2026-07 against Angular 21.2.13 and current official Angular i18n guidance. @angular/localize is not installed, so its exact version remains unknown until adoption. Query Context7 and official Angular docs for the selected version and chosen deployment model.
