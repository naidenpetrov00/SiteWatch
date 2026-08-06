# .NET testing

## Default stack

- Use xUnit v3 for new .NET 10 test projects.
- Use NSubstitute and `NSubstitute.Analyzers.CSharp` for boundary substitutes when installation is approved.
- Use xUnit assertions by default. Add an assertion library only after its value and license are approved.
- Use `Microsoft.AspNetCore.Mvc.Testing` and `WebApplicationFactory<Program>` for API integration tests.
- Prefer the production database engine for EF Core query and persistence tests, using isolated SQL Server state or an approved container fixture.

Never create a home-grown mocking framework. Do not mock `DbSet` queries or use EF Core InMemory as proof of SQL Server behavior. If a database double is unavoidable, mock an existing Application-owned abstraction; do not introduce a repository solely to ease one test.

## Match the layer

### Domain

Instantiate domain objects directly. Test invariants, transitions, value semantics, and failures without DI, mocks, EF Core, or ASP.NET Core.

### Application

Exercise commands, queries, validators, mappings, and policies through public behavior. Substitute only Application-owned ports. Return explicit domain values and verify calls only when delegation is part of the use-case contract.

### Infrastructure

Use integration tests for EF mappings, translated queries, transactions, storage adapters, and external protocols. Test against a faithful disposable service. Keep credentials and repository-persisted data out of tests.

### API

Drive endpoints through `HttpClient` from `WebApplicationFactory`. Assert status, headers, contracts, auth, validation, and visible side effects. Replace only true external services; keep routing, middleware, serialization, and application wiring real.

## xUnit patterns

- Use `[Fact]` for one behavior and `[Theory]` for one rule over cases.
- Return `Task` or `ValueTask` from async tests; never use `async void`.
- Share expensive immutable setup with fixtures. Reset mutable state per test.
- Use collections deliberately when a resource cannot run concurrently.
- Prefer semantic assertions over asserting every property indiscriminately.

## Data and nondeterminism

Use named factories or builders near the tests. Supply fixed timestamps, IDs, and inputs through existing boundaries such as `TimeProvider` or Application-owned generators. Do not weaken encapsulation solely to set private state.

Isolate database state by database/schema, transaction, or reliable reset strategy. Align parallelism with that model. Never target an unnamed or shared environment.

## NSubstitute discipline

- Substitute interfaces or overridable boundary members, not domain behavior.
- Configure only values used by the scenario.
- Prefer exact or constrained arguments over `Arg.Any<T>()`.
- Use `Received` sparingly for meaningful interactions.
- Treat long substitute chains as a design smell.
- Enable the analyzer when approved.

Avoid private-method tests, mirrored production algorithms, mocked logging without an operational contract, test ordering, wall-clock delays, real network services, secrets, and broad snapshots.
