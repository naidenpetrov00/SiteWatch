# Skill Index

## prompt-generator

**Purpose:** Turn repository work requests into concise, execution-ready prompts with relevant skill and model guidance.

**Activates:** The user invokes `/prompt` or asks for a prompt for another implementation agent.

**Validated technologies:** Codex skill format; repository-local model catalog.

**Technical validation:** Not recorded by the existing skill.

**Related:** skill-generator.

**Origin:** Existing repository-local skill.

**Location:** `.codex/skills/prompt-generator/SKILL.md`.

**Status:** Current.

## dashboard-feature-architecture

**Purpose:** Preserve Dashboard feature ownership and core/features/shared boundaries.

**Activates:** Adding, moving, splitting, or reviewing Dashboard pages, services, models, utilities, providers, or shared UI.

**Validated technologies:** Angular 21.2.13; TypeScript 5.9.3.

**Technical validation:** 2026-07.

**Related:** dashboard-components-templates; dashboard-routing-composition; dashboard-models-mapping; dashboard-data-access.

**Origin:** Generated locally from approved Dashboard architecture and current Angular guidance.

**Location:** .codex/skills/dashboard-feature-architecture/SKILL.md.

**Status:** Current.

## dashboard-components-templates

**Purpose:** Build strict standalone components, signal state, directives, and modern templates.

**Activates:** Dashboard component, directive, signal input/output, computed/effect, template, projection, or component-style changes.

**Validated technologies:** Angular 21.2.13; TypeScript 5.9.3.

**Technical validation:** 2026-07.

**Related:** dashboard-feature-architecture; dashboard-material-ui; dashboard-testing.

**Origin:** Generated locally from Dashboard component patterns and current Angular guidance.

**Location:** .codex/skills/dashboard-components-templates/SKILL.md.

**Status:** Current.

## dashboard-routing-composition

**Purpose:** Configure Dashboard bootstrap providers, lazy routes, shells, navigation, and guards.

**Activates:** app.config.ts, app.routes.ts, Router, RouterLink, route data, provider scope, resolver, or guard changes.

**Validated technologies:** Angular Router 21.2.13.

**Technical validation:** 2026-07.

**Related:** dashboard-feature-architecture; dashboard-authentication; dashboard-ssr-hydration; dashboard-testing.

**Origin:** Generated locally from Dashboard routing architecture and current Angular guidance.

**Location:** .codex/skills/dashboard-routing-composition/SKILL.md.

**Status:** Current.

## dashboard-material-ui

**Purpose:** Build accessible Material/CDK interfaces with the Dashboard Material 3 theme.

**Activates:** Angular Material, CDK, shared UI, theming, responsive component CSS, dialog, menu, focus, accessibility, or animation changes.

**Validated technologies:** Angular Material/CDK 21.2.13.

**Technical validation:** 2026-07.

**Related:** dashboard-components-templates; dashboard-forms-dialogs; dashboard-data-tables; dashboard-testing.

**Origin:** Generated locally from Dashboard UI patterns and current official Angular Material guidance.

**Location:** .codex/skills/dashboard-material-ui/SKILL.md.

**Status:** Current.

## dashboard-models-mapping

**Purpose:** Define strict Dashboard wire models and pure boundary transformations.

**Activates:** Request, response, lookup, detail, table-row, mapper, formatter, calculation, date, or number conversion changes.

**Validated technologies:** TypeScript 5.9.3; current SiteWatch Dashboard HTTP contracts.

**Technical validation:** 2026-07.

**Related:** dashboard-data-access; dashboard-forms-dialogs; dashboard-data-tables; dashboard-internationalization.

**Origin:** Generated locally from Dashboard models, mappers, and adjacent API contracts.

**Location:** .codex/skills/dashboard-models-mapping/SKILL.md.

**Status:** Current.

## dashboard-async-ui-state

**Purpose:** Standardize loading, fetching, empty, error, retry, and mutation feedback.

**Activates:** TanStack query status, mutation pending, failed HTTP or dialog operations, retry, disabled actions, or accessible progress and error UI.

**Validated technologies:** Angular 21.2.13; TanStack Angular Query 5.101.0.

**Technical validation:** 2026-07.

**Related:** dashboard-data-access; dashboard-forms-dialogs; dashboard-material-ui; dashboard-testing.

**Origin:** Generated locally from approved Dashboard UX decisions and current TanStack Query guidance.

**Location:** .codex/skills/dashboard-async-ui-state/SKILL.md.

**Status:** Current.

## dashboard-data-access

**Purpose:** Implement typed Dashboard HTTP and TanStack Query server-state flows.

**Activates:** HttpClient, feature services, injectQuery, injectMutation, query keys, parameters, invalidation, API URL, or cache-boundary changes.

**Validated technologies:** Angular HTTP 21.2.13; TanStack Angular Query 5.101.0; RxJS 7.8.2.

**Technical validation:** 2026-07.

**Related:** dashboard-models-mapping; dashboard-async-ui-state; dashboard-authentication; dashboard-ngrx-state.

**Origin:** Generated locally from Dashboard data services, API contracts, and current TanStack Query guidance.

**Location:** .codex/skills/dashboard-data-access/SKILL.md.

**Status:** Current.

## dashboard-authentication

**Purpose:** Maintain secure Dashboard JWT session, interceptor, identity, and guard behavior.

**Activates:** core/auth, identity, sessionStorage, bearer headers, public-request bypass, login/logout, 401/403, or authenticated-route changes.

**Validated technologies:** Angular HTTP/Router 21.2.13; current SiteWatch JWT contract.

**Technical validation:** 2026-07.

**Related:** dashboard-data-access; dashboard-routing-composition; dashboard-async-ui-state; dashboard-ssr-hydration.

**Origin:** Generated locally from Dashboard authentication architecture and current security guidance.

**Location:** .codex/skills/dashboard-authentication/SKILL.md.

**Status:** Current.

## dashboard-forms-dialogs

**Purpose:** Build typed Reactive Forms, validation, dynamic rows, and Material dialog workflows.

**Activates:** FormGroup, FormArray, validator, dialog, wizard, reusable form section, submission, or form-to-request changes.

**Validated technologies:** Angular Forms 21.2.13; Angular Material 21.2.13.

**Technical validation:** 2026-07.

**Related:** dashboard-material-ui; dashboard-models-mapping; dashboard-async-ui-state; dashboard-testing.

**Origin:** Generated locally from approved Reactive Forms architecture and current Angular guidance.

**Location:** .codex/skills/dashboard-forms-dialogs/SKILL.md.

**Status:** Current.

## dashboard-data-tables

**Purpose:** Build reusable server-driven Dashboard filtering, sorting, paging, counts, actions, and exports.

**Activates:** shared/data-table or feature table column, filter, sort, page, count, export, or server query-state changes.

**Validated technologies:** Angular/Material 21.2.13; TanStack Angular Query 5.101.0; SiteWatch TableQueryRequest.

**Technical validation:** 2026-07.

**Related:** dashboard-data-access; dashboard-async-ui-state; dashboard-material-ui; dashboard-testing.

**Origin:** Generated locally from the Dashboard data-table framework and backend table-query contract.

**Location:** .codex/skills/dashboard-data-tables/SKILL.md.

**Status:** Current.

## dashboard-testing

**Purpose:** Add focused Angular Dashboard behavior tests with TestBed and Vitest.

**Activates:** Dashboard behavior changes, regressions, components, signals, services, HTTP, authentication, routing, Material, TanStack Query, or pure utility tests.

**Validated technologies:** Angular build/CLI 21.2.11; Angular 21.2.13; Vitest 4.1.6; jsdom 28.1.0.

**Technical validation:** 2026-07.

**Related:** all Dashboard implementation skills.

**Origin:** Generated locally from Dashboard tests, execution boundaries, and current Angular testing guidance.

**Location:** .codex/skills/dashboard-testing/SKILL.md.

**Status:** Current.

## dashboard-ssr-hydration

**Purpose:** Safely assess and adopt opt-in SSR, hybrid rendering, and client hydration.

**Activates:** Explicit SSR, SSG, hybrid rendering, hydration, event replay, incremental hydration, or hydration-debugging work.

**Validated technologies:** Angular 21.2.13; @angular/ssr not installed and version unknown.

**Technical validation:** 2026-07.

**Related:** dashboard-routing-composition; dashboard-data-access; dashboard-internationalization; dashboard-service-worker.

**Origin:** Generated locally from Dashboard constraints and current official Angular hydration guidance.

**Location:** .codex/skills/dashboard-ssr-hydration/SKILL.md.

**Status:** Current.

## dashboard-service-worker

**Purpose:** Safely assess and adopt opt-in Angular service-worker and PWA behavior.

**Activates:** Explicit service-worker, PWA, offline, caching, update-notification, or service-worker recovery work.

**Validated technologies:** Angular 21.2.13; @angular/service-worker not installed and version unknown.

**Technical validation:** 2026-07.

**Related:** dashboard-data-access; dashboard-authentication; dashboard-internationalization; dashboard-testing.

**Origin:** Generated locally from Dashboard security boundaries and current official Angular service-worker guidance.

**Location:** .codex/skills/dashboard-service-worker/SKILL.md.

**Status:** Current.

## dashboard-internationalization

**Purpose:** Safely assess and adopt Angular localization, locale formatting, and localized builds.

**Activates:** Languages, translations, @angular/localize, message extraction, locale data, localized Material behavior, or locale deployment work.

**Validated technologies:** Angular 21.2.13; @angular/localize not installed and version unknown.

**Technical validation:** 2026-07.

**Related:** dashboard-material-ui; dashboard-models-mapping; dashboard-ssr-hydration; dashboard-service-worker.

**Origin:** Generated locally from Dashboard UI constraints and current official Angular i18n guidance.

**Location:** .codex/skills/dashboard-internationalization/SKILL.md.

**Status:** Current.

## dashboard-ngrx-state

**Purpose:** Safely assess and implement scoped NgRx client and workflow state.

**Activates:** Explicit NgRx work or client state that has outgrown component signals across components, routes, or workflows.

**Validated technologies:** Angular 21.2.13; NgRx not installed and version unknown.

**Technical validation:** 2026-07.

**Related:** dashboard-components-templates; dashboard-data-access; dashboard-async-ui-state; dashboard-testing.

**Origin:** Generated locally from the approved Dashboard state boundary and current official NgRx guidance.

**Location:** .codex/skills/dashboard-ngrx-state/SKILL.md.

**Status:** Current.

## skill-generator

**Purpose:** Analyze a requested software-system area and create or refresh an approved repository-local modular set of coding skills.

**Activates:** Generating skills for a named directory, layer, subsystem, or capability; refreshing scoped skills after dependency or architecture changes.

**Validated technologies:** Codex skill format; Context7 MCP resolve/query workflow documented in Context7 v1.0.14.

**Technical validation:** 2026-07.

**Related:** skill-creator; prompt-generator.

**Origin:** Generated for this repository from user requirements, current skill-creator guidance, and current Context7 documentation.

**Location:** `.codex/skills/skill-generator/SKILL.md`.

**Status:** Current.

## backend-feature-slices

**Purpose:** Orchestrate complete backend features across the four backend layers.

**Activates:** Behavior that spans Domain, Application, Infrastructure, and Api.

**Validated technologies:** .NET 10; ASP.NET Core 10.0.10; EF Core 10.0.10; MediatR 14.2.0; FluentValidation 12.1.1; AutoMapper 16.2.0.

**Technical validation:** 2026-07.

**Related:** domain-entities; mediator-use-cases; application-ports; ef-modeling; minimal-api-endpoints; backend-testing.

**Origin:** Generated locally from repository architecture and current framework guidance; adapted conceptually from the installed create-datadriven-aspnetcore skill.

**Location:** `.codex/skills/backend-feature-slices/SKILL.md`.

**Status:** Current.

## minimal-api-endpoints

**Purpose:** Implement thin, typed, MediatR-backed minimal API endpoint groups.

**Activates:** Route mapping, binding, HTTP results, OpenAPI metadata, or endpoint discovery.

**Validated technologies:** ASP.NET Core 10.0.10; MediatR 14.2.0.

**Technical validation:** 2026-07.

**Related:** http-contracts; api-authorization; api-request-pipeline; file-uploads; mediator-use-cases.

**Origin:** Repository-local adaptation/fork of the installed dotnet-webapi skill.

**Location:** `.codex/skills/minimal-api-endpoints/SKILL.md`.

**Status:** Current.

## http-contracts

**Purpose:** Design stable request, response, pagination, and file contracts.

**Activates:** DTO, serialized-field, status/result shape, or compatibility changes.

**Validated technologies:** ASP.NET Core 10.0.10; AutoMapper 16.2.0.

**Technical validation:** 2026-07.

**Related:** minimal-api-endpoints; dto-mapping; table-queries; api-request-pipeline.

**Origin:** Repository-local adaptation/fork of the installed dotnet-webapi skill.

**Location:** `.codex/skills/http-contracts/SKILL.md`.

**Status:** Current.

## api-request-pipeline

**Purpose:** Maintain middleware ordering, centralized errors, diagnostics, CORS, and OpenAPI pipeline behavior.

**Activates:** Program.cs, middleware, error-envelope, or cross-cutting HTTP changes.

**Validated technologies:** ASP.NET Core 10.0.10.

**Technical validation:** 2026-07.

**Related:** http-contracts; api-authorization; backend-composition.

**Origin:** Repository-local adaptation/fork of the installed dotnet-webapi skill.

**Location:** `.codex/skills/api-request-pipeline/SKILL.md`.

**Status:** Current.

## api-authorization

**Purpose:** Enforce endpoint and application-request access rules.

**Activates:** Authentication requirements, roles, claims, policies, ownership, 401, or 403 behavior.

**Validated technologies:** ASP.NET Core Authentication/Authorization 10.0.10; ASP.NET Core Identity 10.0.10; System.IdentityModel.Tokens.Jwt 8.19.2.

**Technical validation:** 2026-07.

**Related:** identity-integration; minimal-api-endpoints; api-request-pipeline.

**Origin:** Generated locally from repository security boundaries and current framework guidance.

**Location:** `.codex/skills/api-authorization/SKILL.md`.

**Status:** Current.

## file-uploads

**Purpose:** Implement secure multipart upload and streamed download boundaries.

**Activates:** IFormFile, multipart binding, file validation, request limits, antiforgery, or stream ownership.

**Validated technologies:** ASP.NET Core 10.0.10; Azure.Storage.Blobs 12.29.1.

**Technical validation:** 2026-07.

**Related:** minimal-api-endpoints; validation-pipeline; application-ports; blob-storage.

**Origin:** Repository-local adaptation/fork of the installed minimal-api-file-upload skill.

**Location:** `.codex/skills/file-uploads/SKILL.md`.

**Status:** Current.

## mediator-use-cases

**Purpose:** Implement commands, queries, handlers, and application orchestration.

**Activates:** IRequest, IRequestHandler, use-case, cancellation, or handler-structure changes.

**Validated technologies:** .NET 10; MediatR 14.2.0.

**Technical validation:** 2026-07.

**Related:** validation-pipeline; application-ports; ef-queries; ef-writes-transactions; dto-mapping.

**Origin:** Generated locally from repository Application patterns and current framework guidance.

**Location:** `.codex/skills/mediator-use-cases/SKILL.md`.

**Status:** Current.

## validation-pipeline

**Purpose:** Implement FluentValidation request rules through the MediatR pipeline.

**Activates:** Request validation, async existence checks, conditional/collection rules, or validation behavior.

**Validated technologies:** FluentValidation 12.1.1; MediatR 14.2.0; EF Core 10.0.10.

**Technical validation:** 2026-07.

**Related:** mediator-use-cases; domain-entities; api-request-pipeline.

**Origin:** Generated locally from repository validation patterns and current framework guidance.

**Location:** `.codex/skills/validation-pipeline/SKILL.md`.

**Status:** Current.

## dto-mapping

**Purpose:** Map domain/query data into safe DTOs with explicit or AutoMapper projections.

**Activates:** DTO mapping, nested profiles, ProjectTo, computed fields, or translation failures.

**Validated technologies:** AutoMapper 16.2.0; EF Core 10.0.10.

**Technical validation:** 2026-07.

**Related:** http-contracts; ef-queries; table-queries.

**Origin:** Generated locally from repository mapping patterns and current framework guidance.

**Location:** `.codex/skills/dto-mapping/SKILL.md`.

**Status:** Current.

## table-queries

**Purpose:** Build allow-listed filtered, sorted, counted, and paged dashboard queries.

**Activates:** TableQueryRequest, TableQueryDefinition, table DTO, or paged dashboard endpoint changes.

**Validated technologies:** EF Core 10.0.10; MediatR 14.2.0.

**Technical validation:** 2026-07.

**Related:** ef-queries; validation-pipeline; dto-mapping; minimal-api-endpoints.

**Origin:** Generated locally from the repository's table-query framework.

**Location:** `.codex/skills/table-queries/SKILL.md`.

**Status:** Current.

## domain-entities

**Purpose:** Model entities, aggregate behavior, invariants, collections, and domain events.

**Activates:** Entity factory, state-transition, invariant, relationship, audit, or domain-event changes.

**Validated technologies:** .NET 10; Ardalis.GuardClauses 5.0.0; MediatR 14.2.0.

**Technical validation:** 2026-07.

**Related:** value-objects; ef-modeling; ef-writes-transactions; backend-testing.

**Origin:** Generated locally from repository Domain patterns.

**Location:** `.codex/skills/domain-entities/SKILL.md`.

**Status:** Current.

## value-objects

**Purpose:** Model normalized domain concepts with structural equality and persistence compatibility.

**Activates:** New or changed value object, equality, normalization, conversion, or owned mapping.

**Validated technologies:** .NET 10; Ardalis.GuardClauses 5.0.0; EF Core 10.0.10.

**Technical validation:** 2026-07.

**Related:** domain-entities; ef-modeling; backend-testing.

**Origin:** Generated locally from repository Domain value-object patterns.

**Location:** `.codex/skills/value-objects/SKILL.md`.

**Status:** Current.

## ef-modeling

**Purpose:** Configure EF Core entities, relationships, constraints, value objects, indexes, and sequences.

**Activates:** ApplicationDbContext or IEntityTypeConfiguration changes.

**Validated technologies:** EF Core SQL Server 10.0.10; ASP.NET Core Identity EF Core 10.0.10.

**Technical validation:** 2026-07.

**Related:** domain-entities; value-objects; ef-schema-changes; ef-queries; ef-writes-transactions.

**Origin:** Generated locally from repository persistence architecture and current framework guidance.

**Location:** `.codex/skills/ef-modeling/SKILL.md`.

**Status:** Current.

## ef-queries

**Purpose:** Write efficient, translated, bounded EF Core read queries.

**Activates:** New/slow queries, N+1 risk, tracking, projection, paging, or generated SQL concerns.

**Validated technologies:** EF Core 10.0.10; AutoMapper 16.2.0.

**Technical validation:** 2026-07.

**Related:** dto-mapping; table-queries; ef-modeling.

**Origin:** Repository-local adaptation/fork of the installed optimizing-ef-core-queries skill.

**Location:** `.codex/skills/ef-queries/SKILL.md`.

**Status:** Current.

## ef-writes-transactions

**Purpose:** Implement reliable tracked/set-based writes, transactions, concurrency, and domain-event behavior.

**Activates:** Create/update/delete, SaveChanges, manual transaction, concurrency, or partial-failure work.

**Validated technologies:** EF Core SQL Server 10.0.10; MediatR 14.2.0.

**Technical validation:** 2026-07.

**Related:** domain-entities; ef-modeling; application-ports; backend-testing.

**Origin:** Generated locally from repository write patterns and current framework guidance.

**Location:** `.codex/skills/ef-writes-transactions/SKILL.md`.

**Status:** Current.

## ef-schema-changes

**Purpose:** Assess and safely manage model deltas, migration artifacts, and database-application approvals.

**Activates:** Table, column, key, relationship, index, sequence, conversion, migration, or snapshot changes.

**Validated technologies:** .NET SDK 10.0.302; EF Core SQL Server/Design/Tools 10.0.10.

**Technical validation:** 2026-07.

**Related:** ef-modeling; ef-writes-transactions.

**Origin:** Generated locally from repository migration policy and current framework guidance.

**Location:** `.codex/skills/ef-schema-changes/SKILL.md`.

**Status:** Current.

## application-ports

**Purpose:** Define Application-owned capability interfaces and Infrastructure adapters.

**Activates:** New or changed persistence, identity, storage, email, device, invoice, or other external boundary.

**Validated technologies:** .NET 10; current Application/Infrastructure architecture.

**Technical validation:** 2026-07.

**Related:** mediator-use-cases; backend-composition; ef-writes-transactions; blob-storage; email-delivery.

**Origin:** Generated locally from repository dependency boundaries.

**Location:** `.codex/skills/application-ports/SKILL.md`.

**Status:** Current.

## backend-composition

**Purpose:** Compose dependency injection, assembly scanning, lifetimes, and startup wiring.

**Activates:** DependencyInjection.cs, Program.cs, service registration, or lifetime changes.

**Validated technologies:** Microsoft.Extensions.DependencyInjection 10.0.10; MediatR 14.2.0; AutoMapper 16.2.0; FluentValidation 12.1.1.

**Technical validation:** 2026-07.

**Related:** application-ports; options-configuration; api-request-pipeline; identity-integration.

**Origin:** Generated locally from repository composition roots and current framework guidance.

**Location:** `.codex/skills/backend-composition/SKILL.md`.

**Status:** Current.

## options-configuration

**Purpose:** Bind and validate typed backend configuration without exposing secrets.

**Activates:** Option classes, appsettings sections, environment variables, binding, or validation.

**Validated technologies:** Microsoft.Extensions.Configuration/DependencyInjection 10.0.10.

**Technical validation:** 2026-07.

**Related:** backend-composition; identity-integration; blob-storage; email-delivery.

**Origin:** Generated locally from repository configuration conventions and current framework guidance.

**Location:** `.codex/skills/options-configuration/SKILL.md`.

**Status:** Current.

## identity-integration

**Purpose:** Implement secure Identity and JWT authentication/account workflows.

**Activates:** Signup/signin, verification/reset, users, roles, claims, JWT creation, or JWT validation.

**Validated technologies:** ASP.NET Core Identity/JwtBearer 10.0.10; System.IdentityModel.Tokens.Jwt 8.19.2.

**Technical validation:** 2026-07.

**Related:** api-authorization; api-request-pipeline; options-configuration; email-delivery.

**Origin:** Generated locally from repository Identity architecture and current security guidance.

**Location:** `.codex/skills/identity-integration/SKILL.md`.

**Status:** Current.

## blob-storage

**Purpose:** Implement Azure Blob Storage operations behind Application ports.

**Activates:** Blob client registration, upload/download/delete, streams, containers, or compensation.

**Validated technologies:** Azure.Storage.Blobs 12.29.1; .NET 10.

**Technical validation:** 2026-07.

**Related:** application-ports; file-uploads; options-configuration; ef-writes-transactions.

**Origin:** Generated locally from repository storage adapters and current SDK guidance.

**Location:** `.codex/skills/blob-storage/SKILL.md`.

**Status:** Current.

## email-delivery

**Purpose:** Implement secure transactional email and identity-message workflows.

**Activates:** SMTP, email ports/adapters, templates, verification/reset mail, or email side effects.

**Validated technologies:** .NET 10 System.Net.Mail; ASP.NET Core Identity 10.0.10.

**Technical validation:** 2026-07.

**Related:** identity-integration; options-configuration; application-ports; ef-writes-transactions.

**Origin:** Generated locally from repository email architecture and current security guidance.

**Location:** `.codex/skills/email-delivery/SKILL.md`.

**Status:** Current.

## backend-testing

**Purpose:** Add focused tests for backend domain, application, API, persistence, and adapter behavior.

**Activates:** Backend behavior changes, regressions, validators, mappings, queries, authorization, or adapters.

**Validated technologies:** xUnit 2.9.2; Microsoft.NET.Test.Sdk 17.12.0; net9.0 test project; net10.0 backend.

**Technical validation:** 2026-07.

**Related:** all backend implementation skills.

**Origin:** Generated locally from repository test conventions and execution boundaries.

**Location:** `.codex/skills/backend-testing/SKILL.md`.

**Status:** Current.

## agents-file-generator

**Purpose:** Analyze and generate a scoped AGENTS.md instruction hierarchy.

**Activates:** Creating, reviewing, restructuring, or refreshing repository agent instructions.

**Validated technologies:** Codex AGENTS.md and repository-local skill routing.

**Technical validation:** Not recorded by the existing skill.

**Related:** skill-generator.

**Origin:** Existing repository-local skill; registered without changing its instructions.

**Location:** `.codex/skills/agents-file-generator/SKILL.md`.

**Status:** Current.
