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
