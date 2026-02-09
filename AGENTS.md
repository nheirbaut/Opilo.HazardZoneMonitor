# Agent Rules

## Hard Rules

- **NEVER install, update, or remove software on the system without explicit user permission.** This includes package managers (winget, choco, npm -g, pip, etc.), CLI tools, SDKs, runtimes, and any other system-level software. Always ask first.
- **Strict TDD.** Do not generate production code unless explicitly requested. Follow `.copilot/tests-only-agent.prompt.md` for test generation.
- **No MediatR, no SignalR, no AutoMapper or similar object-mapping libraries (e.g., Mapster).** These are explicitly banned.
- **Always load the `solid` skill** when delegating code tasks. Use `load_skills=["solid"]` for every `task()` call that writes, refactors, reviews, or architects code.

## Project Overview

Opilo HazardZone Monitor — a safety application monitoring restricted/hazardous areas for unauthorized entries. .NET 10, Vertical Slice Architecture, DDD, CQRS, Dapper + Sqlite.

### Structure

```
src/
  Opilo.HazardZoneMonitor.Domain/        # Rich domain models, events, state machine, shared primitives
  Opilo.HazardZoneMonitor.Api/           # Minimal API, vertical slices (Features/), CQRS handlers
tests/
  Opilo.HazardZoneMonitor.Domain.Tests.Unit/    # Domain unit tests (xUnit v3, AwesomeAssertions)
  Opilo.HazardZoneMonitor.Api.Tests.Unit/       # API handler unit tests (xUnit v3, NSubstitute)
  Opilo.HazardZoneMonitor.Tests.Integration/    # Integration tests (WebApplicationFactory, xUnit v3)
```

Each API feature slice lives in `Features/{FeatureArea}/{UseCaseName}/` containing: `Feature.cs` (DI + endpoint), `Handler.cs`, `Command.cs`/`Query.cs`, `Response.cs`.

## Build / Test Commands

```sh
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/Opilo.HazardZoneMonitor.Domain.Tests.Unit
dotnet test tests/Opilo.HazardZoneMonitor.Api.Tests.Unit
dotnet test tests/Opilo.HazardZoneMonitor.Tests.Integration

# Run a single test by fully qualified name
dotnet test --filter "FullyQualifiedName~HazardZoneTests.Constructor_ShouldThrowArgumentNullException_WhenNameIsNull"

# Run tests matching a pattern
dotnet test --filter "FullyQualifiedName~HazardZoneTests"

# Run the API
dotnet run --project src/Opilo.HazardZoneMonitor.Api
```

**Warnings are errors.** `TreatWarningsAsErrors` and `CodeAnalysisTreatWarningsAsErrors` are both `true`. Code style is enforced at build time via `EnforceCodeStyleInBuild`.

## Analyzers

All projects use: Meziantou.Analyzer, Microsoft.CodeAnalysis.NetAnalyzers (latest/All), Roslynator, SecurityCodeScan, SonarAnalyzer.CSharp. Fix all warnings — they break the build.

## Code Style

### Formatting

- 4 spaces indentation, LF line endings, UTF-8, final newline required
- Braces on new line (Allman style) for all constructs
- File-scoped namespaces (`namespace X;`)
- XML/csproj files use 2-space indent

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Classes, records, enums | PascalCase | `HazardZone`, `Location` |
| Interfaces | `I` + PascalCase | `IClock`, `ICommandHandler` |
| Public methods/properties | PascalCase | `HandlePersonCreated`, `ZoneState` |
| Private fields | `_camelCase` | `_currentState`, `_zoneStateLock` |
| Static private fields | `s_camelCase` | `s_instance` |
| Constants | PascalCase | `DefaultName` |
| Parameters, locals | camelCase | `personId`, `locationInsideZone` |
| Test methods | `Method_ShouldExpected_WhenCondition` | `Constructor_ShouldThrowArgumentNullException_WhenNameIsNull` |

### Types and Language

- **Nullable reference types**: enabled project-wide — respect them, do not suppress
- **Implicit usings**: enabled — do not add `using System;` etc.
- Prefer language keywords over BCL types (`string` not `String`, `int` not `Int32`)
- Avoid `var` — use explicit types (configured: `csharp_style_var_for_built_in_types = false`)
- Prefer `readonly` fields
- Avoid `this.` qualifier
- Sort `using` directives with `System` first, place outside namespace

### Records and DTOs

- Use `record` for immutable value objects and DTOs: `record Location(double X, double Y)`
- Use `record` for commands/queries/responses: `record Command(Guid PersonId, double X, double Y) : ICommand<Response>`
- Use `sealed class` for domain entities with behavior

### Error Handling

- Use `Ardalis.GuardClauses` for argument validation (`Guard.Against.Null`, `Guard.Against.NullOrWhiteSpace`, `Guard.Against.Negative`)
- Use `Ardalis.Result<T>` for handler return types — not exceptions for flow control
- Domain objects throw `ArgumentException`/`ArgumentNullException` for invalid construction
- API handlers return `Result.Created()`, `Result.Success()`, etc.

### Dependency Injection

- Each feature slice registers its own services via `IFeature.AddServices(IServiceCollection)`
- Features are auto-discovered from the assembly via `AddFeaturesFromAssembly()`
- CQRS: `ICommandHandler<TCommand, TResponse>` and `IQueryHandler<TQuery, TResponse>` — no MediatR
- Register handlers as `Scoped`

### Logging

- Serilog with Console and File sinks
- Configured via `appsettings.json` (`ReadFrom.Configuration`)
- Bootstrap logger only in non-Development environments

### Domain Patterns

- Rich domain models — no anemic models. Behavior lives on the entity.
- State pattern for HazardZone alarm management (base class `HazardZoneStateBase`, concrete states)
- Domain events via standard .NET `event EventHandler<TEventArgs>` — not a mediator
- EventArgs as records: `record PersonAddedToHazardZoneEventArgs(Guid PersonId, string HazardZoneName)`
- Thread safety with `Lock` and `lock` blocks where needed
- Clock/Timer abstractions (`IClock`, `ITimer`, `ITimerFactory`) for testability

### Configuration

- `IOptions<T>` pattern bound from configuration files
- Floor definitions, HazardZones, timeouts come from config — not the database
- Validate at startup, not at runtime

## Test Conventions

- **Framework**: xUnit v3 (uses `xunit.v3` package, NOT xunit v2)
- **Assertions**: AwesomeAssertions (NOT FluentAssertions) — globally imported
- **Mocking**: NSubstitute for API tests; hand-written fakes (`FakeClock`, `FakeTimer`, `FakeTimerFactory`) for domain tests
- **Style**: Arrange-Act-Assert with `// Arrange`, `// Act`, `// Assert` comments
- **Test class naming**: `{ClassUnderTest}Tests` for unit tests, `{Feature}Specification` for integration tests
- **Test class structure**: `sealed class` implementing `IDisposable` when SUT needs disposal
- **Builder pattern**: `HazardZoneBuilder.Create().WithState(...).Build()` for complex test setup
- **Helper extensions**: `GetLocationInside()`, `GetLocationOutside()` on builder for readable tests
- **Integration tests**: Use `CustomWebApplicationFactory` with `IClassFixture<>`, primary constructor injection
- **Cancellation**: Use `TestContext.Current.CancellationToken` in async tests (xUnit v3 pattern)
- Test behavior, not implementation details. Mock only at boundaries.

### London-Style TDD

This project uses strict London-style TDD (Red -> Green -> Refactor). When writing tests:
1. Propose the next test first, keep it as small as possible
2. Mock at boundaries (ports/interfaces), not internal domain objects
3. Verify externally observable behavior: returned results, persisted state, emitted events
4. Avoid asserting implementation details

## Package Management

Central Package Management (`Directory.Packages.props`). When adding packages, add `PackageVersion` there and use versionless `PackageReference` in project files.

## Suppressed Analyzer Rules

- `CA1040` (empty interfaces): suppressed — `IApiMarker`, `IFeature` are intentional
- `CA1062` (validate public args): suppressed — redundant with nullable reference types
- `CA1716` (keyword identifiers): suggestion only — `Shared` namespace is intentional
- `CA1707` (underscores in names): suppressed in tests — test methods use underscores
- `CA1515` (public types): suppressed for Web SDK and test projects
- `CA2007` (ConfigureAwait): suggestion only in API and tests — no SynchronizationContext in ASP.NET Core
- `MA0004`: suggestion only in API — same rationale as CA2007
