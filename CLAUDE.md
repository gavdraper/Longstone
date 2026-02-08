# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Longstone is a UK portfolio management system built with .NET 9 and Aspire. It covers fund setup, order management, compliance, dealing, settlement, NAV calculation, performance measurement, and regulatory reporting under UK (FCA/HMRC) rules. See [PRD.md](PRD.md) for full requirements.

## Build & Run Commands

```bash
# Build
dotnet build Longstone.sln

# Run all tests
dotnet test Longstone.sln

# Run a single test project
dotnet test tests/Longstone.Domain.Tests

# Run a single test by name
dotnet test tests/Longstone.Domain.Tests --filter "FullyQualifiedName~UserTests.Create_WithValidInputs"

# Run the app (Aspire orchestrator)
dotnet run --project src/Longstone.AppHost
```

Build is configured with `TreatWarningsAsErrors=true` and `EnforceCodeStyleInBuild=true` via `Directory.Build.props`. All warnings must be resolved before code compiles.

## Architecture

**Clean Architecture with DDD**, organized by bounded context:

```
src/
  Longstone.AppHost/          → Aspire orchestrator (entry point)
  Longstone.ServiceDefaults/  → Shared Aspire config (OpenTelemetry, health checks, resilience)
  Longstone.Web/              → Blazor Server UI (MudBlazor) + API endpoints
  Longstone.Application/      → Use cases via MediatR commands/queries, FluentValidation
  Longstone.Domain/           → Entities, value objects, enums, interfaces (zero dependencies)
  Longstone.Infrastructure/   → EF Core (SQLite), external API integrations

tests/
  Longstone.Domain.Tests/        → Domain unit tests
  Longstone.Application.Tests/   → Application layer tests
  Longstone.Integration.Tests/   → Integration tests
```

**Dependency flow:** AppHost → Web → Application → Domain ← Infrastructure

Domain has no external dependencies. Infrastructure depends on both Domain and Application.

## Domain Model Conventions

- **Entities**: Private parameterless constructor (for EF Core), `static Create(...)` factory method, private setters
- **Value objects**: `sealed record` with static factory methods
- **Time**: Always inject `TimeProvider` — never use `DateTime.UtcNow` directly. Tests use `FakeTimeProvider`
- **Validation**: Guard clauses in factory methods (`ArgumentException.ThrowIfNullOrWhiteSpace`, `ArgumentNullException.ThrowIfNull`)
- **Marker interfaces**: `IAuditable` for entities requiring audit tracking
- **Organization**: Code organized by bounded context (`Auth/`, `Audit/`, `Common/`), not by technical concern

### Permission System

Three-tier resolution: user override > role default > deny. Permissions have scopes (`Own`, `All`). See `PermissionGrant.Resolve()`.

## Testing Conventions

- **Framework**: xUnit + FluentAssertions + NSubstitute (configured in `tests/Directory.Build.props`)
- **TDD**: Write tests first for domain logic and business rules
- **Naming**: `MethodName_Scenario_ExpectedResult` with underscores (e.g., `Create_WithValidInputs_SetsAllProperties`)
- **File naming**: `{Subject}Tests.cs`, mirroring source structure
- **Time testing**: Use `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider`

## Tech Stack

| Concern | Technology |
|---------|-----------|
| Orchestration | .NET Aspire 9.2 |
| UI | Blazor Server + MudBlazor 8.15 |
| CQRS | MediatR 14 |
| Validation | FluentValidation 12 |
| ORM | EF Core 9 (SQLite, designed for SQL Server swap — no raw SQL) |
| Observability | OpenTelemetry via Aspire ServiceDefaults |
| Testing | xUnit + FluentAssertions + NSubstitute |

## Key Design Decisions

- **SQLite first**: Using SQLite for v1 with a migration path to SQL Server. All queries must use EF Core LINQ (no raw SQL) to keep the database swappable.
- **Aspire-first**: OpenTelemetry tracing, metrics, and logging are configured through Aspire ServiceDefaults. All services should call `AddServiceDefaults()`.
- **Audit everything**: Immutable `AuditEvent` entities capture before/after state as JSON. Entities implementing `IAuditable` will be auto-tracked via EF Core interceptor.
- **Blazor Server**: Server-rendered with built-in SignalR for real-time features (dealer blotter). No separate SPA.
- **Deployment target**: Docker on UGREEN NAS with Tailscale Funnel for HTTPS.
