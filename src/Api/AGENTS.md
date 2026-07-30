# API layer instructions

## Scope and ownership

- This file applies to `src/Api`.
- Api is the ASP.NET Core host and outer backend layer. It owns startup/composition, dependency registration, middleware, authentication/authorization configuration, and HTTP endpoints.
- Keep endpoints thin: translate transport concerns and delegate application behavior through the established application boundary.
- Do not place persistence queries, EF configuration, external-system implementation, or domain business rules in Api.

## Task guidance

- Inspect the relevant endpoint group, nearby endpoint conventions, `Program.cs`, and dependency-registration files only when the task touches those responsibilities.
- Treat authentication, authorization, request contracts, response contracts, and status semantics as externally observable behavior. Check affected Dashboard or ClientApp scopes only when the request changes their contract.
- Use the `dotnet-aspnetcore:dotnet-webapi` skill for Web API endpoint implementation when applicable, plus other narrowly relevant .NET skills. Use Context7 only for gaps left unresolved by repository context and skills.
