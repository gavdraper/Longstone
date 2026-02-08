# Longstone Foundation — Slices 0 & 1

**Created**: 2026-02-08
**Status**: Phase 14 Complete (10.5 requires manual NAS deploy)
**Estimated Complexity**: High

## Objective

Build the Longstone portfolio management system in two vertical slices:

- **Slice 0**: Deployable shell — get a running app on the NAS with auth, permissions, theming, telemetry, and deployment automation. Zero business logic, fully shippable.
- **Slice 1**: Fund CRUD + Instrument reference data — first real feature end-to-end with pluggable instrument architecture and per-user permission overrides UI.

Priority is deployment infrastructure first so we iterate on real hardware immediately.

## Current State Analysis

Greenfield repository. Only `PRD.md` (full requirements) and `README.md` exist. No code, no solution file.

**Environment confirmed:**
- .NET SDK 9.0.306
- Aspire templates available (v13.1.0)
- MudBlazor latest: 8.15.0
- NuGet source: nuget.org

## Approach

**Vertical slices** — each slice delivers DB → Domain → Application → Infrastructure → UI → Deployment as a single unit.

**TDD** — write tests first for domain logic and command handlers, then implement to green.

**Feature-based organization** — code organized by business domain, not technical layer within each project.

**Pluggable instruments** — strategy pattern keyed by `AssetClass` for valuation, tax, and compliance behaviors. New instruments = new enum value + strategy implementations + DI registration.

**Permissions** — role defaults from PRD matrix seeded in `RolePermission` table. Per-user overrides in `UserPermissionOverride` table. Resolution: user override > role default > deny. Admin UI to manage overrides.

## Key Decisions

- **MudBlazor 8.x** for UI components (data grids, forms, dialogs, theming)
- **Dev-mode cookie auth** — seeded users with hashed passwords, swap to OIDC later by changing auth config only
- **aspire-starter template as base** — gives us AppHost + ServiceDefaults + Web project scaffolding, then we add Domain/Application/Infrastructure class libraries
- **xUnit + FluentAssertions + NSubstitute** for testing
- **FluentValidation** for command validation via MediatR pipeline
- **SQLite WAL mode** for concurrent read performance; all access via EF Core LINQ (no raw SQL)
- **Audit interceptor** — EF Core `SaveChangesInterceptor` auto-captures before/after state on `IAuditable` entities

## Risks & Considerations

- **MudBlazor 8.x breaking changes**: v8 has different API from v7 tutorials. Use official v8 docs only.
  - Mitigation: Pin exact version, check migration guide
- **Aspire 13.x**: Newer than most tutorials. Template structure may differ.
  - Mitigation: Use `dotnet new` templates directly, verify generated structure
- **SQLite concurrency**: Limited write concurrency under load.
  - Mitigation: WAL mode, and this is v1 — SQL Server migration path is designed in
- **Tailscale deployment**: Requires auth key and NAS SSH access.
  - Mitigation: Script handles all steps, .env.example documents required vars

## Implementation Steps

### Phase 1: Solution Scaffolding (Slice 0)

- [x] 1.1 Create solution using `aspire-starter` template
- [x] 1.2 Add class library projects: `Longstone.Domain`, `Longstone.Application`, `Longstone.Infrastructure`
- [x] 1.3 Add test projects: `Longstone.Domain.Tests`, `Longstone.Application.Tests`, `Longstone.Integration.Tests`
- [x] 1.4 Configure project references:
  - `Web` → Application, Infrastructure, ServiceDefaults
  - `Application` → Domain
  - `Infrastructure` → Domain, Application
  - `AppHost` → Web (Aspire orchestration ref)
  - Each test project → corresponding src project
- [x] 1.5 Add NuGet packages:
  - `Web`: MudBlazor (8.15.0), MediatR, Microsoft.AspNetCore.Authentication.Cookies, FluentValidation.DependencyInjectionExtensions
  - `Application`: MediatR.Contracts, FluentValidation
  - `Infrastructure`: Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design
  - `Domain`: (none — pure domain)
  - Test projects: xUnit, FluentAssertions, NSubstitute, Microsoft.NET.Test.Sdk
  - `Integration.Tests`: Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory
- [x] 1.6 Verify solution builds: `dotnet build`

### Phase 2: Domain Model — Auth & Audit (Slice 0)

- [x] 2.1 **Write tests**: `tests/Longstone.Domain.Tests/Auth/RoleTests.cs` — enum values match PRD roles
- [x] 2.2 **Write tests**: `tests/Longstone.Domain.Tests/Auth/PermissionResolutionTests.cs` — override > role default > deny logic
- [x] 2.3 Implement domain enums and value objects:
  - `src/Longstone.Domain/Auth/Role.cs` — SystemAdmin, FundManager, Dealer, ComplianceOfficer, Operations, RiskManager, ReadOnly
  - `src/Longstone.Domain/Auth/Permission.cs` — ViewPortfolios, CreateOrders, ExecuteOrders, ConfigureCompliance, OverrideComplianceBreach, ProcessCorporateActions, RunNavCalculation, ViewRiskDashboards, ManageUsers, ViewAuditLogs
  - `src/Longstone.Domain/Auth/PermissionScope.cs` — Own, All
  - `src/Longstone.Domain/Auth/PermissionGrant.cs` — value object encapsulating permission + scope resolution
  - `src/Longstone.Domain/Auth/PermissionGrantSource.cs` — Default, RoleDefault, UserOverride
- [x] 2.4 Implement auth entities:
  - `src/Longstone.Domain/Auth/User.cs` — Id (Guid), Username, Email, FullName, Role, IsActive, PasswordHash, CreatedAt, UpdatedAt
  - `src/Longstone.Domain/Auth/RolePermission.cs` — Id, Role, Permission, Scope (represents the default matrix)
  - `src/Longstone.Domain/Auth/UserPermissionOverride.cs` — Id, UserId, Permission, Scope, IsGranted, OverriddenBy, OverriddenAt, Reason
- [x] 2.5 Implement audit entity:
  - `src/Longstone.Domain/Audit/AuditEvent.cs` — Id (Guid), Timestamp, UserId, UserRole, Action, EntityType, EntityId, BeforeState (JSON), AfterState (JSON), Reason, IpAddress, SessionId
  - `src/Longstone.Domain/Common/IAuditable.cs` — marker interface
- [x] 2.6 Verify tests pass: `dotnet test`

### Phase 3: EF Core + SQLite (Slice 0)

- [x] 3.1 Implement DbContext:
  - `src/Longstone.Infrastructure/Persistence/LongstoneDbContext.cs` — DbSets for User, RolePermission, UserPermissionOverride, AuditEvent
  - Configure SQLite WAL mode via connection string pragma
- [x] 3.2 Implement entity configurations:
  - `src/Longstone.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
  - `src/Longstone.Infrastructure/Persistence/Configurations/RolePermissionConfiguration.cs`
  - `src/Longstone.Infrastructure/Persistence/Configurations/UserPermissionOverrideConfiguration.cs`
  - `src/Longstone.Infrastructure/Persistence/Configurations/AuditEventConfiguration.cs`
- [x] 3.3 Implement `DesignTimeDbContextFactory.cs` for migration CLI
- [x] 3.4 Create initial migration: `dotnet ef migrations add InitialAuth`
- [x] 3.5 Implement seed data:
  - `src/Longstone.Infrastructure/Persistence/Seed/UserSeedData.cs` — 7 users (one per role), password `Dev123!` hashed with ASP.NET Core Identity hasher
  - `src/Longstone.Infrastructure/Persistence/Seed/RolePermissionSeedData.cs` — full matrix from PRD Section 2.2
- [x] 3.6 Implement `src/Longstone.Infrastructure/DependencyInjection.cs` — registers DbContext, applies migrations on startup, seeds data
- [x] 3.7 Verify migrations apply and seed runs

### Phase 4: Aspire ServiceDefaults + AppHost (Slice 0)

- [x] 4.1 Configure `src/Longstone.ServiceDefaults/Extensions.cs`:
  - OpenTelemetry traces (ASP.NET Core, EF Core, HttpClient, custom `ActivitySource("Longstone")`)
  - OpenTelemetry metrics (ASP.NET Core, custom `Meter("Longstone")`)
  - Structured logging with trace correlation
  - OTLP exporter via `OTEL_EXPORTER_OTLP_ENDPOINT`
  - Health checks: `/health` (liveness), `/alive` (readiness)
  - Resilience defaults (HttpClient retry, circuit breaker)
- [x] 4.2 Configure `src/Longstone.AppHost/Program.cs`:
  - Reference Web project
  - Wire up service discovery
- [x] 4.3 Verify `dotnet run --project src/Longstone.AppHost` starts and Aspire Dashboard shows

### Phase 5: MudBlazor Theming + Layout Shell (Slice 0)

- [x] 5.1 Install and configure MudBlazor in `src/Longstone.Web`:
  - Add MudBlazor services in `Program.cs`
  - Add MudBlazor CSS/JS references in `App.razor` or `_Host.cshtml`
  - Configure `MudThemeProvider` with custom theme
- [x] 5.2 Define custom theme:
  - `src/Longstone.Web/Theme/LongstoneTheme.cs`
  - Professional palette: dark indigo sidebar, clean white/light gray content, accent blue for actions
  - Dark mode variant with proper contrast
  - Typography: clean sans-serif, readable data tables
- [x] 5.3 Implement main layout:
  - `src/Longstone.Web/Components/Layout/MainLayout.razor` — MudLayout with MudDrawer (sidebar) + MudAppBar (top bar)
  - `src/Longstone.Web/Components/Layout/NavMenu.razor` — MudNavMenu with role-aware items (hide based on permissions)
  - Sidebar nav items: Dashboard, Funds, Instruments, Orders (disabled), Compliance (disabled), Risk (disabled), Reports (disabled), Admin section (Users, Permissions, Audit Log)
  - Top bar: app logo/name, theme toggle (dark/light via MudToggleIconButton), user display name + role badge, logout button
- [x] 5.4 Implement placeholder pages:
  - `src/Longstone.Web/Components/Pages/Home.razor` — dashboard placeholder with welcome message
  - Disabled nav items show "Coming Soon" page
- [x] 5.5 Verify app renders with themed layout

### Phase 6: Cookie Authentication + Login Page (Slice 0)

- [x] 6.1 **Write tests**: `tests/Longstone.Integration.Tests/Auth/AuthenticationTests.cs` — login success, login failure, logout, redirect to login when unauthenticated
- [x] 6.2 Implement auth service:
  - `src/Longstone.Infrastructure/Auth/AuthenticationService.cs` — `IAuthenticationService` interface in Domain, implementation in Infrastructure
  - Validates username + password against DB (ASP.NET Core Identity password hasher)
  - Returns user claims (UserId, Username, FullName, Role)
- [x] 6.3 Configure cookie auth in `Program.cs`:
  - `AddAuthentication(CookieAuthenticationDefaults)` + `AddCookie()`
  - Login path: `/auth/login`
  - Session timeout: 8 hours (configurable)
  - Sliding expiration enabled
- [x] 6.4 Implement login page:
  - `src/Longstone.Web/Components/Pages/Auth/Login.razor` — MudCard centered, MudTextField for username/password, MudButton submit
  - Clean, professional design with Longstone branding
  - Error display on invalid credentials
  - Redirect to dashboard on success
- [x] 6.5 Implement login/logout API endpoints (minimal API) — `POST /api/auth/login` and `POST /api/auth/logout` with antiforgery disabled
- [x] 6.6 Add `[Authorize]` attribute to MainLayout to protect all pages
- [x] 6.7 Verify tests pass, manual login flow works

### Phase 7: Permission System (Slice 0)

- [x] 7.1 **Write tests**: `tests/Longstone.Domain.Tests/Auth/PermissionResolutionTests.cs` (expand from 2.2):
  - User with no overrides gets role defaults
  - User with grant override gets access beyond role
  - User with deny override loses access from role
  - SystemAdmin always has all permissions
  - Scope resolution (Own vs All) works correctly
- [x] 7.2 **Write tests**: `tests/Longstone.Integration.Tests/Auth/PermissionAuthorizationTests.cs`:
  - Fund manager can access own funds page
  - Fund manager cannot access admin pages
  - Fund manager WITH override can access all portfolios
  - Denied override blocks even role-default access
- [x] 7.3 Implement permission service:
  - `src/Longstone.Domain/Auth/IPermissionService.cs` — interface
  - `src/Longstone.Infrastructure/Auth/PermissionService.cs` — implementation
  - `HasPermissionAsync(Guid userId, Permission permission)` → bool
  - `GetPermissionScopeAsync(Guid userId, Permission permission)` → PermissionScope?
  - `GetEffectivePermissionsAsync(Guid userId)` → list of (Permission, Scope, Source) for UI display
  - Resolution: check UserPermissionOverride first → fall back to RolePermission → deny
- [x] 7.4 Implement ASP.NET Core authorization integration:
  - `src/Longstone.Infrastructure/Auth/PermissionRequirement.cs` — `IAuthorizationRequirement` with Permission property
  - `src/Longstone.Infrastructure/Auth/PermissionAuthorizationHandler.cs` — `AuthorizationHandler<PermissionRequirement>` that calls PermissionService
  - Register policies in `Program.cs`: one policy per permission (e.g., `"Permission:ViewPortfolios"`)
- [x] 7.5 Wire NavMenu to hide items user lacks permission for (inject PermissionService, check in OnInitializedAsync)
- [x] 7.6 Verify all tests pass

### Phase 8: Audit Trail Infrastructure (Slice 0)

- [x] 8.1 **Write tests**: `tests/Longstone.Integration.Tests/Audit/AuditInterceptorTests.cs` — creating/updating an auditable entity produces audit events with correct before/after state
- [x] 8.2 Implement EF Core audit interceptor:
  - `src/Longstone.Infrastructure/Persistence/Interceptors/AuditSaveChangesInterceptor.cs`
  - On `SavingChangesAsync`: detect Added/Modified/Deleted entries for `IAuditable` entities
  - Capture before state (original values JSON) and after state (current values JSON)
  - Create `AuditEvent` with user context from `IHttpContextAccessor`
  - Write audit events to DbContext in same transaction
- [x] 8.3 Register interceptor in DbContext configuration
- [x] 8.4 Verify tests pass

### Phase 9: Dockerfile + Docker Compose + Deploy Script (Slice 0)

- [x] 9.1 Create `Dockerfile` (repo root):
  - Build stage: `mcr.microsoft.com/dotnet/sdk:9.0-alpine`, copy solution, restore, publish Web project
  - Runtime stage: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine`, port 8080, `/app/data` volume mount point
  - ENTRYPOINT `dotnet Longstone.Web.dll`
- [x] 9.2 Create `.dockerignore` — exclude .git, bin, obj, .claude, tests, *.md
- [x] 9.3 Create `docker-compose.hub.yml`:
  - `tailscale` service: image `tailscale/tailscale:latest`, env `TS_AUTHKEY`, `TS_EXTRA_ARGS=--advertise-tags=tag:container`, `TS_SERVE_CONFIG=/config/tailscale-serve.json`, volumes for state + serve config
  - `longstone` service: built image `longstone:latest`, `network_mode: service:tailscale`, volume `longstone-data:/app/data`, env vars for OTLP + ASP.NET Core, health check via wget, restart always
  - External network `observability-net`
  - Named volumes: `longstone-data`, `tailscale-state`
- [x] 9.4 Create `tailscale-serve.json`:
  - TCP 443 → HTTPS, proxy to `http://127.0.0.1:8080`
  - AllowFunnel: true
- [x] 9.5 Create `.env.example` with documented variables
- [x] 9.6 Create `scripts/deploy-to-nas.sh`:
  - Configurable: NAS_HOST, NAS_USER, NAS_DEPLOY_PATH, IMAGE_NAME (from .env or script defaults)
  - Steps: build image with git SHA tag → docker save → scp to NAS → ssh load → ssh docker compose up -d → poll health check → print URL
  - Error handling at each step, colored output
  - `chmod +x`
- [x] 9.7 Verify Docker build succeeds locally: `docker build -t longstone:dev .`
- [x] 9.8 Verify container starts and health check passes: `docker run -p 8080:8080 longstone:dev`

### Phase 10: Slice 0 Final Verification

- [x] 10.1 Run full test suite: `dotnet test` — 119 tests (76 domain + 15 application + 28 integration), all passing
- [x] 10.2 Run app via Aspire: `dotnet run --project src/Longstone.AppHost` — builds with 0 warnings, requires manual Aspire Dashboard verification
- [x] 10.3 Manual smoke test: login as each role, verify nav items, verify unauthorized access blocked (see checklist below)
- [x] 10.4 Docker build + run smoke test — 174MB image, health checks pass (/health, /alive return 200)
- [ ] **10.5 Deploy to NAS** (first real deployment)

---

### Phase 11: Instrument Domain Model (Slice 1)

- [x] 11.1 **Write tests**: `tests/Longstone.Domain.Tests/Instruments/InstrumentTests.cs`:
  - Instrument creation with valid properties
  - Instrument status transitions (Active → Suspended → Delisted)
  - Asset class classification
- [x] 11.2 **Write tests**: `tests/Longstone.Application.Tests/Instruments/InstrumentStrategyTests.cs` (moved to Application.Tests — tests concrete Infrastructure implementations):
  - Equity valuation strategy returns quantity × price
  - Equity tax strategy applies 0.5% stamp duty for UK equities
  - ETF tax strategy does not apply stamp duty
- [x] 11.3 Implement instrument domain:
  - `src/Longstone.Domain/Instruments/Instrument.cs` — Id (Guid), Isin, Sedol, Ticker, Exchange, Name, Currency, CountryOfListing, Sector, AssetClass, MarketCapitalisation, Status, CreatedAt, UpdatedAt. Implements `IAuditable`.
  - `src/Longstone.Domain/Instruments/AssetClass.cs` — enum: Equity, FixedIncome, ETF, Fund, Cash, Alternative
  - `src/Longstone.Domain/Instruments/InstrumentStatus.cs` — enum: Active, Suspended, Delisted
  - `src/Longstone.Domain/Instruments/Exchange.cs` — enum: LSE, NYSE, NASDAQ, Euronext, XETRA
- [x] 11.4 Implement pluggable strategy interfaces:
  - `src/Longstone.Domain/Instruments/Strategies/IInstrumentValuationStrategy.cs` — `decimal CalculateMarketValue(decimal quantity, decimal price)`; `decimal CalculateAccruedIncome(...)` (default no-op for equities)
  - `src/Longstone.Domain/Instruments/Strategies/IInstrumentTaxStrategy.cs` — `decimal CalculateStampDuty(decimal consideration, Instrument instrument)`; `TaxTreatment GetDividendTaxTreatment(Instrument instrument)`
  - `src/Longstone.Domain/Instruments/Strategies/IInstrumentComplianceStrategy.cs` — `bool IsEligibleForFund(Instrument instrument)`; `IEnumerable<string> GetComplianceFlags(Instrument instrument)`
  - `src/Longstone.Domain/Instruments/Strategies/TaxTreatment.cs` — enum: Taxable, TaxExempt, TaxDeferred, WithholdingTax
- [x] 11.5 Implement concrete strategies:
  - `src/Longstone.Infrastructure/Instruments/Strategies/EquityValuationStrategy.cs`
  - `src/Longstone.Infrastructure/Instruments/Strategies/EquityTaxStrategy.cs` — 0.5% SDRT for UK LSE, 0% for non-UK
  - `src/Longstone.Infrastructure/Instruments/Strategies/EtfValuationStrategy.cs`
  - `src/Longstone.Infrastructure/Instruments/Strategies/EtfTaxStrategy.cs` — no stamp duty
  - `src/Longstone.Infrastructure/Instruments/Strategies/DefaultComplianceStrategy.cs`
- [x] 11.6 Register strategies in DI keyed by `AssetClass`
- [x] 11.7 Verify tests pass — 188 total (121 domain + 39 application + 28 integration), all green, 0 warnings

### Phase 12: Fund + Mandate Domain Model (Slice 1)

- [x] 12.1 **Write tests**: `tests/Longstone.Domain.Tests/Funds/FundTests.cs`:
  - Fund creation with valid properties
  - Fund status transitions (Active → Suspended → Closed)
  - Fund manager assignment
  - Cannot create fund without base currency
- [x] 12.2 **Write tests**: `tests/Longstone.Domain.Tests/Compliance/MandateRuleTests.cs`:
  - Rule creation with valid parameters
  - Rule activation/deactivation with effective dates
  - Hard vs Soft severity
- [x] 12.3 Implement fund domain:
  - `src/Longstone.Domain/Funds/Fund.cs` — Id (Guid), Name, Lei, Isin, FundType, BaseCurrency, BenchmarkIndex, InceptionDate, Status, CreatedAt, UpdatedAt. Implements `IAuditable`. Navigation: AssignedManagers (collection of User), MandateRules.
  - `src/Longstone.Domain/Funds/FundType.cs` — enum: OEIC, UnitTrust, InvestmentTrust, SegregatedMandate
  - `src/Longstone.Domain/Funds/FundStatus.cs` — enum: Active, Suspended, Closed
  - `src/Longstone.Domain/Funds/FundManager.cs` — join entity: FundId, UserId
- [x] 12.4 Implement mandate domain:
  - `src/Longstone.Domain/Compliance/MandateRule.cs` — Id (Guid), FundId, RuleType, Parameters (JSON string), Severity, IsActive, EffectiveFrom, EffectiveTo, CreatedAt, UpdatedAt. Implements `IAuditable`.
  - `src/Longstone.Domain/Compliance/MandateRuleType.cs` — enum: MaxSingleStockWeight, MaxSectorExposure, MaxCountryExposure, MinCashHolding, BannedInstrument, AssetClassLimit, MarketCapFloor, MaxHoldings, CurrencyExposureLimit, TrackingErrorLimit
  - `src/Longstone.Domain/Compliance/RuleSeverity.cs` — enum: Hard, Soft
- [x] 12.5 Verify tests pass — 333 total (248 domain + 15 application + 42 infrastructure + 28 integration), all green, 0 warnings

### Phase 13: EF Core Configuration + Migration (Slice 1)

- [x] 13.1 Add entity configurations:
  - `src/Longstone.Infrastructure/Persistence/Configurations/InstrumentConfiguration.cs` — indexes on Isin, Sedol, Ticker; unique constraint on Isin
  - `src/Longstone.Infrastructure/Persistence/Configurations/FundConfiguration.cs` — index on Name; many-to-many with User for managers
  - `src/Longstone.Infrastructure/Persistence/Configurations/MandateRuleConfiguration.cs` — FK to Fund
  - `src/Longstone.Infrastructure/Persistence/Configurations/FundManagerConfiguration.cs` — join table config
- [x] 13.2 Add DbSets to LongstoneDbContext: Instruments, Funds, FundManagers, MandateRules
- [x] 13.3 Create migration: `dotnet ef migrations add AddFundsAndInstruments`
- [x] 13.4 Implement instrument seed data:
  - `src/Longstone.Infrastructure/Persistence/Seed/InstrumentSeedData.cs`
  - ~50 instruments: top 30 FTSE 100 stocks, 10 major US equities (AAPL, MSFT, AMZN, GOOGL, META, NVDA, TSLA, JPM, V, JNJ), 5 ETFs (VWRL, ISF, CSPX, VUSA, EQQQ), 5 gilts/bonds
  - Each with realistic ISIN, SEDOL, sector, exchange, market cap
- [x] 13.5 Verify migration applies and seed runs — 339 tests (254 domain + 15 application + 42 infrastructure + 28 integration), all green, 0 warnings

### Phase 14: MediatR Pipeline + Repository Layer (Slice 1)

- [x] 14.1 Implement MediatR pipeline behaviors:
  - `src/Longstone.Application/Common/Behaviors/ValidationBehavior.cs` — runs FluentValidation before handler
  - `src/Longstone.Application/Common/Behaviors/AuditBehavior.cs` — logs command execution to audit trail (command name, user, timestamp)
  - `src/Longstone.Application/Common/Behaviors/PerformanceBehavior.cs` — logs slow commands (>500ms) as warnings
- [x] 14.2 Define repository interfaces in Domain:
  - `src/Longstone.Domain/Funds/IFundRepository.cs` — CRUD + query by manager, by status
  - `src/Longstone.Domain/Instruments/IInstrumentRepository.cs` — search by text (name/ticker/ISIN/SEDOL), filter by asset class/exchange/country
  - `src/Longstone.Domain/Compliance/IMandateRuleRepository.cs` — by fund, active rules only
- [x] 14.3 Implement repositories in Infrastructure:
  - `src/Longstone.Infrastructure/Persistence/Repositories/FundRepository.cs`
  - `src/Longstone.Infrastructure/Persistence/Repositories/InstrumentRepository.cs`
  - `src/Longstone.Infrastructure/Persistence/Repositories/MandateRuleRepository.cs`
- [x] 14.4 Register MediatR + pipeline behaviors + repositories in DI
- [x] 14.5 Verify build succeeds — 339 tests (254 domain + 15 application + 42 infrastructure + 28 integration), all green, 0 warnings

### Phase 15: Fund Commands & Queries (Slice 1)

- [ ] 15.1 **Write tests**: `tests/Longstone.Application.Tests/Funds/CreateFundHandlerTests.cs`:
  - Creates fund with valid data
  - Fails validation when name is empty
  - Fails validation when base currency missing
  - Produces audit event
- [ ] 15.2 **Write tests**: `tests/Longstone.Application.Tests/Funds/GetFundsHandlerTests.cs`:
  - Returns paginated results
  - Filters by status
  - Fund manager scoping: returns only assigned funds when scope is Own
- [ ] 15.3 Implement fund commands:
  - `src/Longstone.Application/Funds/Commands/CreateFund/CreateFundCommand.cs` — record with all fund properties
  - `src/Longstone.Application/Funds/Commands/CreateFund/CreateFundCommandHandler.cs`
  - `src/Longstone.Application/Funds/Commands/CreateFund/CreateFundValidator.cs`
  - `src/Longstone.Application/Funds/Commands/UpdateFund/UpdateFundCommand.cs` + Handler + Validator
  - `src/Longstone.Application/Funds/Commands/ChangeFundStatus/ChangeFundStatusCommand.cs` + Handler
- [ ] 15.4 Implement fund queries:
  - `src/Longstone.Application/Funds/Queries/GetFunds/GetFundsQuery.cs` — Page, PageSize, StatusFilter, SearchTerm
  - `src/Longstone.Application/Funds/Queries/GetFunds/GetFundsHandler.cs` — returns `PaginatedList<FundDto>`
  - `src/Longstone.Application/Funds/Queries/GetFundById/GetFundByIdQuery.cs` + Handler
  - `src/Longstone.Application/Common/Models/PaginatedList.cs` — generic paginated result
  - `src/Longstone.Application/Funds/Queries/FundDto.cs` — projection DTO
- [ ] 15.5 Verify tests pass

### Phase 16: Instrument Queries (Slice 1)

- [ ] 16.1 **Write tests**: `tests/Longstone.Application.Tests/Instruments/SearchInstrumentsHandlerTests.cs`:
  - Search by ticker returns match
  - Search by ISIN returns match
  - Filter by asset class works
  - Empty search returns paginated all
- [ ] 16.2 Implement instrument queries:
  - `src/Longstone.Application/Instruments/Queries/SearchInstruments/SearchInstrumentsQuery.cs` — SearchTerm, AssetClassFilter, ExchangeFilter, Page, PageSize
  - `src/Longstone.Application/Instruments/Queries/SearchInstruments/SearchInstrumentsHandler.cs`
  - `src/Longstone.Application/Instruments/Queries/GetInstrumentById/GetInstrumentByIdQuery.cs` + Handler
  - `src/Longstone.Application/Instruments/Queries/InstrumentDto.cs`
- [ ] 16.3 Verify tests pass

### Phase 17: Mandate Commands & Queries (Slice 1)

- [ ] 17.1 **Write tests**: `tests/Longstone.Application.Tests/Mandates/AddMandateRuleHandlerTests.cs`:
  - Adds rule with valid parameters
  - Validates rule type + parameters compatibility
  - Produces audit event
- [ ] 17.2 Implement mandate commands:
  - `src/Longstone.Application/Mandates/Commands/AddMandateRule/AddMandateRuleCommand.cs` + Handler + Validator
  - `src/Longstone.Application/Mandates/Commands/UpdateMandateRule/UpdateMandateRuleCommand.cs` + Handler
  - `src/Longstone.Application/Mandates/Commands/ToggleMandateRule/ToggleMandateRuleCommand.cs` + Handler
- [ ] 17.3 Implement mandate queries:
  - `src/Longstone.Application/Mandates/Queries/GetMandateRules/GetMandateRulesQuery.cs` — FundId, ActiveOnly
  - `src/Longstone.Application/Mandates/Queries/GetMandateRules/GetMandateRulesHandler.cs`
  - `src/Longstone.Application/Mandates/Queries/MandateRuleDto.cs`
- [ ] 17.4 Verify tests pass

### Phase 18: Blazor UI — Fund Pages (Slice 1)

- [ ] 18.1 Implement Fund List page:
  - `src/Longstone.Web/Components/Pages/Funds/FundList.razor`
  - MudDataGrid with `ServerData` for server-side pagination/sorting/filtering
  - Columns: Name, Type (chip), Base Currency, Benchmark, Status (colored chip), Manager(s)
  - Search bar (debounced), status filter dropdown
  - "New Fund" FAB button (gated by `Permission:CreateOrders` — only fund managers + admin)
  - Row click → navigate to `/funds/{id}`
- [ ] 18.2 Implement Fund Detail page:
  - `src/Longstone.Web/Components/Pages/Funds/FundDetail.razor`
  - MudTabs: Properties | Mandate Rules | Holdings (placeholder "Coming in next slice") | Cash (placeholder)
  - Properties tab: MudForm with fields matching Fund entity, MudSelect for FundType/Currency/Status, MudDatePicker for inception
  - Save button → calls UpdateFund command
  - Permission-gated: read-only view for users without edit permission
- [ ] 18.3 Implement Create Fund dialog:
  - `src/Longstone.Web/Components/Pages/Funds/CreateFundDialog.razor`
  - MudDialog with form fields, validation, submit → CreateFund command
- [ ] 18.4 Implement Mandate Rules tab:
  - `src/Longstone.Web/Components/Pages/Funds/MandateRulesTab.razor`
  - MudDataGrid: Rule Type, Parameters (formatted), Severity (chip), Active (switch), Effective From/To
  - "Add Rule" button → MudDialog with RuleType dropdown, parameter fields (dynamic based on type), severity, dates
  - Toggle active/inactive inline
  - Gated by `Permission:ConfigureCompliance`

### Phase 19: Blazor UI — Instrument Browser (Slice 1)

- [ ] 19.1 Implement Instrument List page:
  - `src/Longstone.Web/Components/Pages/Instruments/InstrumentList.razor`
  - MudDataGrid with server-side data, search across name/ticker/ISIN/SEDOL
  - Columns: Name, Ticker, ISIN, Exchange, Sector, Asset Class (chip), Currency, Market Cap (formatted), Status
  - Filter chips/dropdowns: Asset Class, Exchange, Country
  - Responsive: works on wide screens for data-heavy view

### Phase 20: Admin — Permission Override UI (Slice 1)

- [ ] 20.1 Implement User Permissions page:
  - `src/Longstone.Web/Components/Pages/Admin/UserPermissions.razor`
  - MudSelect to pick a user
  - MudTable showing all permissions: Permission Name | Role Default (read-only, from RolePermission) | Effective (computed) | Override (tri-state: Inherit / Grant / Deny)
  - Each row has a MudSelect or toggle group for override state
  - Optional scope selector (Own/All) when granting
  - Reason field (MudTextField) required when overriding
  - Save button → writes UserPermissionOverride records
  - Gated by `Permission:ManageUsers`

### Phase 21: Admin — Audit Log Viewer (Slice 1)

- [ ] 21.1 Implement Audit Log page:
  - `src/Longstone.Web/Components/Pages/Admin/AuditLog.razor`
  - MudDataGrid with server-side pagination
  - Columns: Timestamp, User, Action, Entity Type, Entity ID
  - Row expand → shows Before/After JSON state diff
  - Filters: date range, user, entity type, action type
  - Read-only — no one can modify audit events

### Phase 22: Slice 1 Final Verification

- [ ] 22.1 Run full test suite: `dotnet test` — all green
- [ ] 22.2 Run app via Aspire — full smoke test:
  - Login as admin → create a fund → add mandate rules
  - Login as fund manager → see only assigned funds (if scoped) → view fund detail
  - Browse instruments → search by ticker
  - Admin → override permissions for fund manager → verify access changes
  - Admin → view audit log → see all mutations logged
- [ ] 22.3 Docker build + run smoke test
- [ ] **22.4 Deploy to NAS** — full Slice 1 live

## Test Coverage

**Unit tests (Domain + Application):**
- Permission resolution logic (override > role > deny)
- Fund entity validation and state transitions
- Instrument classification and strategies
- Mandate rule creation and validation
- All MediatR command handlers (with mocked repositories)
- All MediatR query handlers (with mocked repositories)
- Validation behaviors (FluentValidation)

**Integration tests:**
- Authentication flow (login/logout/redirect)
- Permission authorization (role defaults + overrides)
- Audit interceptor (creates events on entity changes)
- Full CRUD flows with real SQLite DB

**Edge cases:**
- Concurrent permission override + role default resolution
- Empty search results (instruments, funds)
- Fund with no mandate rules
- User with all permissions denied via overrides
- SystemAdmin bypasses all permission checks
- Pagination boundary conditions (last page, page beyond count)

## Success Criteria

- [ ] App builds with zero warnings
- [ ] All tests pass
- [ ] Login works for all 7 seeded users
- [ ] Each role sees appropriate nav items and is blocked from unauthorized pages
- [ ] Admin can override permissions per-user and the change takes effect immediately
- [ ] Fund CRUD works end-to-end (list, create, edit, status change)
- [ ] Mandate rules can be added/edited/toggled per fund
- [ ] Instruments are browsable and searchable
- [ ] All mutations appear in audit log
- [ ] OpenTelemetry traces visible in Aspire Dashboard
- [ ] Docker image builds < 200MB
- [ ] `deploy-to-nas.sh` deploys successfully
- [ ] App accessible via Tailscale with HTTPS

## Notes

- MudBlazor 8.x docs: https://mudblazor.com — pin to 8.15.0
- Aspire 13.1.0 — use `dotnet new aspire-starter` as base
- All EF Core access via LINQ — no raw SQL, no provider-specific features (SQLite → SQL Server swap path)
- Instrument strategy registration: `services.AddKeyedScoped<IInstrumentValuationStrategy, EquityValuationStrategy>(AssetClass.Equity)` — keyed services (.NET 8+)
- Password hashing: use `Microsoft.AspNetCore.Identity.PasswordHasher<User>` directly without full Identity framework
