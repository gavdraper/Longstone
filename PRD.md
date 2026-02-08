# Longstone - Product Requirements Document

## Portfolio Management System for Investment Management

**Version:** 1.0
**Date:** 2026-02-08
**Status:** Draft

---

## 1. Overview

### 1.1 What Is Longstone?

Longstone is a portfolio management system built for UK-based investment management houses. It provides fund managers, dealers, compliance officers, operations staff, and risk managers with a unified platform to manage global investment portfolios under UK regulatory and tax frameworks.

### 1.2 Problem Statement

Small-to-mid-size investment management firms operate with fragmented tooling: spreadsheets for compliance checks, separate systems for order management, manual processes for NAV calculation, and disconnected reporting. This creates operational risk, compliance gaps, and inefficiency.

Longstone consolidates portfolio construction, order management, compliance, performance measurement, and reporting into a single system purpose-built for UK-regulated firms managing global portfolios.

### 1.3 Target Users

| Role | Primary Responsibilities |
|------|------------------------|
| **Fund Manager** | Portfolio construction, investment decisions, rebalancing, performance monitoring |
| **Dealer/Trader** | Order execution, managing the dealing blotter, trade allocation |
| **Compliance Officer** | Mandate rule configuration, breach monitoring, pre/post-trade review |
| **Operations** | Settlement tracking, reconciliation, corporate actions, NAV calculation |
| **Risk Manager** | Exposure monitoring, concentration analysis, risk reporting |

### 1.4 Key Principles

- **Global portfolios, UK rules** — trade any global market, but compliance, tax, and reporting follow FCA and HMRC requirements
- **Audit everything** — every state change is recorded with who, what, when, and why
- **Compliance is not optional** — pre-trade checks gate order flow, post-trade monitoring catches drift
- **Real-time where it matters** — dealer blotter and price feeds are live; reporting can be end-of-day
- **Four-eyes principle** — sensitive operations require a second approver

---

## 2. User Roles & Permissions

### 2.1 Role Hierarchy

```
System Admin
  |-- Compliance Officer
  |-- Risk Manager
  |-- Fund Manager
  |-- Dealer
  |-- Operations
  |-- Read-Only (auditors, client reporting)
```

### 2.2 Permission Matrix

| Capability | Fund Mgr | Dealer | Compliance | Operations | Risk | Admin |
|-----------|----------|--------|------------|------------|------|-------|
| View portfolios | Own funds | All | All | All | All | All |
| Create order instructions | Yes | No | No | No | No | No |
| Execute orders | No | Yes | No | No | No | No |
| Configure compliance rules | No | No | Yes | No | No | Yes |
| Override compliance breach | No | No | Yes | No | No | No |
| Process corporate actions | No | No | No | Yes | No | No |
| Run NAV calculation | No | No | No | Yes | No | Yes |
| View risk dashboards | Yes | Yes | Yes | No | Yes | Yes |
| Manage users & roles | No | No | No | No | No | Yes |
| View audit logs | No | No | Yes | Yes | Yes | Yes |

### 2.3 Authentication & Authorisation

- OpenID Connect / OAuth 2.0 for authentication
- Role-based access control (RBAC) with per-fund scoping for fund managers
- Fund managers only see funds assigned to them
- All other roles have cross-fund visibility appropriate to their function
- Session timeout and re-authentication for sensitive operations

---

## 3. Core Domain: Funds & Portfolios

### 3.1 Fund Entity

Each fund represents a distinct investment vehicle with its own mandate, benchmark, and regulatory constraints.

**Fund Properties:**
- Fund name, legal entity identifier (LEI), ISIN (if applicable)
- Fund type: OEIC, Unit Trust, Investment Trust, Segregated Mandate
- Base currency (GBP, USD, EUR, etc.)
- Benchmark index (e.g., FTSE All-Share, MSCI World, S&P 500)
- Inception date
- Assigned fund manager(s)
- Status: Active, Suspended, Closed
- AUM (Assets Under Management) — derived from holdings + cash

### 3.2 Investment Mandate

Each fund has a configurable investment mandate defining what it can and cannot hold. This is the source of truth for the compliance engine.

**Mandate Constraints (configurable per fund):**

| Constraint Type | Example |
|----------------|---------|
| Max single stock weight | No single equity > 10% of NAV |
| Max sector exposure | No sector > 25% of NAV |
| Max country exposure | No single country > 40% of NAV |
| Min cash holding | Cash must be >= 2% of NAV |
| Banned instruments | Cannot hold tobacco, weapons, gambling stocks |
| Asset class limits | Max 20% in fixed income, max 5% in alternatives |
| Market cap floor | No stocks with market cap < 500m |
| Max number of holdings | Portfolio limited to 50 stocks |
| Currency exposure limit | Max 30% unhedged non-GBP exposure |
| Tracking error limit | Must stay within 3% tracking error of benchmark |

### 3.3 Holdings & Positions

A position represents the current holding of a specific instrument within a fund.

**Position Properties:**
- Instrument identifier (ISIN, SEDOL, ticker, exchange)
- Quantity held
- Average cost (calculated via Section 104 pooling for UK tax purposes)
- Current market value (quantity x last price)
- Weight in fund (market value / fund NAV)
- Unrealised P&L
- Sector, country, currency, asset class classifications
- Accrued income (for fixed income / dividend-paying stocks)

### 3.4 Cash Management

- Each fund has one or more cash accounts (multi-currency)
- Cash is affected by: trades settling, dividends received, management fees, subscriptions/redemptions
- Projected cash view: current cash +/- pending settlements +/- expected income
- Cash is included in NAV and compliance calculations

### 3.5 Model Portfolios

Fund managers can define model portfolios representing target allocations.

- Target weights per instrument or sector
- Drift tolerance thresholds (e.g., rebalance when any holding drifts > 2% from target)
- Rebalancing proposal generation: system calculates orders needed to bring portfolio back to model
- Fund manager reviews and approves proposed orders before they enter the order lifecycle

---

## 4. Order Management System (OMS)

### 4.1 Order Lifecycle

```
                                    +---> Rejected (compliance breach, no override)
                                    |
Draft --> Pre-Trade Compliance --+---> Approved --> Sent to Dealer --> Working
                                                                        |
                    +---> Partially Filled --+                          |
                    |                        |                          |
                    +------------------------+--- Filled --> Allocated --> Settling --> Settled
                                                                                        |
                                                                                   Positions Updated
```

**Order States:**

| State | Description |
|-------|------------|
| **Draft** | Fund manager is composing the order, not yet submitted |
| **Pending Compliance** | Submitted, awaiting pre-trade compliance check |
| **Compliance Breach** | Pre-trade check failed, awaiting compliance officer review |
| **Approved** | Compliance passed (or breach overridden), ready for dealer |
| **Sent to Dealer** | Visible on dealer blotter, awaiting execution |
| **Working** | Dealer has begun executing in market |
| **Partially Filled** | Some quantity executed, remainder still working |
| **Filled** | Entire quantity executed |
| **Allocated** | For block orders: fills allocated across funds |
| **Settling** | Trade awaiting settlement (T+1 UK equities, T+2 international) |
| **Settled** | Cash and positions have been updated |
| **Cancelled** | Order withdrawn before execution |
| **Rejected** | Compliance breach with no override granted |

### 4.2 Order Instruction (Fund Manager Creates)

**Required Fields:**
- Fund
- Instrument (searched by name, ticker, ISIN, or SEDOL)
- Direction: Buy or Sell
- Order type: Market, Limit (with limit price), or At Close
- Quantity or Value (e.g., "buy 10,000 shares" or "buy GBP 50,000 worth")
- Urgency: Normal, Urgent
- Rationale (free text, captured for audit)

**Optional Fields:**
- Time in force: Day, Good Till Cancelled, Good Till Date
- Special instructions to dealer (free text)

### 4.3 Block Orders & Allocation

When a fund manager (or multiple fund managers) want to trade the same instrument:

1. Dealer can aggregate orders into a **block order** for a single market execution
2. Block is executed as one trade to achieve better pricing
3. After fill, the dealer **allocates** fills across participating funds
4. Allocation must be **fair and pro-rata** based on original order sizes
5. All allocation decisions are audited
6. Each fund receives its portion at the block average price

### 4.4 Dealer Blotter

The dealer's primary working screen, updated in real-time via WebSocket:

- All orders in states: Sent to Dealer, Working, Partially Filled
- Sortable/filterable by fund, instrument, urgency, age
- Actions: Acknowledge, Begin Working, Record Fill (price, quantity, venue), Record Partial Fill, Cancel
- Fill recording captures: execution price, quantity, execution venue, timestamp, counterparty (optional)
- Running summary: total orders, total value working, fills today

### 4.5 Order Amendment & Cancellation

- Fund managers can amend or cancel orders in Draft or Approved state
- Once Working, cancellation requires dealer acknowledgment
- All amendments create a new audit entry preserving the previous state
- Cancelled orders retain full history and reason for cancellation

---

## 5. Compliance Engine

### 5.1 Pre-Trade Compliance

Runs automatically when an order instruction is submitted. Simulates the portfolio as if the order were executed and checks all mandate constraints.

**Check Process:**
1. Take current positions for the fund
2. Apply the proposed order (simulate the fill)
3. Re-price all positions using latest market data
4. Calculate new weights, sector exposures, country exposures, cash position
5. Evaluate every active mandate rule
6. Return: Pass (all rules satisfied) or Breach (list of violated rules with details)

**On Breach:**
- Order is held in "Compliance Breach" state
- Notification sent to assigned compliance officer(s)
- Compliance officer can:
  - **Override** the breach with a documented reason (captured in audit)
  - **Reject** the order with a reason (fund manager is notified)
  - **Request amendment** (send back to fund manager to modify)

### 5.2 Post-Trade Compliance

Runs on a scheduled basis (e.g., end-of-day) and on-demand. Checks the actual state of all fund positions against mandate rules.

**Post-Trade Checks Cover:**
- Market movements causing passive breaches (stock price rises push weight above limit)
- Corporate actions changing portfolio composition
- Currency movements affecting exposure limits

**Breach Handling:**
- Passive breaches (caused by market movement, not by trading) are flagged but treated differently from active breaches
- Compliance officer reviews and documents whether corrective action is needed
- If correction required, fund manager is notified with a deadline

### 5.3 Compliance Rule Configuration

Compliance officers can create and manage rules through the UI:

- Rules are scoped to a specific fund or applied as firm-wide defaults
- Each rule has: type, parameters, severity (Hard — blocks trading, Soft — warning only)
- Rules can be activated/deactivated with effective dates
- Rule change history is fully audited

### 5.4 Banned & Restricted Lists

- **Banned list**: instruments that cannot be held or traded under any circumstances
- **Restricted list**: instruments requiring compliance pre-approval before trading
- Lists are maintained globally and can be overridden per fund
- Sources: internal policy, ESG screens, regulatory requirements, insider trading restrictions

---

## 6. Market Data & Instrument Reference

### 6.1 Instrument Reference Data

Every tradeable instrument in the system has a master record:

**Instrument Properties:**
- Primary identifiers: ISIN, SEDOL, ticker
- Exchange (LSE, NYSE, NASDAQ, Euronext, etc.)
- Name, currency, country of listing
- Sector (GICS or ICB classification)
- Asset class: Equity, Fixed Income, ETF, Fund, Cash
- Market capitalisation
- Status: Active, Suspended, Delisted

### 6.2 Market Data Integration

**Real-Time Data (via WebSocket):**
- Last price, bid, ask for instruments currently held or being traded
- Used for: dealer blotter pricing, intraday portfolio valuation
- Source: Finnhub or Twelve Data WebSocket feed
- Graceful degradation: if feed drops, show last known price with staleness indicator

**End-of-Day Data (via REST API):**
- Official closing prices for all held instruments
- Used for: NAV calculation, compliance checks, performance calculation
- Source: same provider, REST endpoints
- Scheduled pull at market close + configurable time (e.g., 18:00 UTC)

**Benchmark Data:**
- Daily index levels for benchmarks: FTSE 100, FTSE 250, FTSE All-Share, MSCI World, S&P 500
- Index constituent weights (where available on free tier, otherwise manual upload)
- Used for: performance attribution, tracking error calculation

### 6.3 Data Caching & Resilience

- All market data is cached locally in the database
- Stale data thresholds: real-time data flagged stale after 5 minutes, EOD data after 24 hours
- API rate limiting handled with backoff and queue
- If external API is unavailable, system continues with cached prices and flags data age
- OpenTelemetry traces on all external API calls for latency and error monitoring

---

## 7. Performance & Attribution

### 7.1 Fund Performance Calculation

**Time-Weighted Return (TWR):**
- Industry standard for measuring fund manager performance (eliminates impact of external cash flows)
- Calculated daily using the modified Dietz method for sub-period returns
- Chained across periods: daily returns compound to produce MTD, QTD, YTD, since-inception returns

**Calculation Inputs:**
- Start-of-day NAV
- End-of-day NAV
- Cash flows during the day (subscriptions, redemptions)

### 7.2 Benchmark Comparison

- Relative return: fund return minus benchmark return
- Excess return displayed for all standard periods
- Tracking error: annualised standard deviation of daily relative returns (rolling 12-month)
- Information ratio: annualised excess return / tracking error

### 7.3 Performance Attribution

Simple Brinson-style attribution at sector level:

- **Allocation effect**: was the fund over/underweight the right sectors?
- **Selection effect**: within each sector, did the fund pick the right stocks?
- **Interaction effect**: combined impact of allocation and selection
- Attribution calculated relative to the fund's assigned benchmark

### 7.4 Performance Screens

- Fund-level dashboard: return chart vs benchmark, drawdown chart, rolling returns
- Holdings-level contribution: which stocks contributed most/least to returns over a period
- Peer comparison: rank fund returns against other funds managed by the firm

---

## 8. Risk Management

### 8.1 Exposure Analysis

**Calculated per fund, refreshed intraday and end-of-day:**

| Metric | Description |
|--------|------------|
| Gross exposure | Sum of absolute values of all position weights |
| Net exposure | Sum of signed position weights (long - short) |
| Single stock concentration | Weight of largest N holdings |
| Sector exposure | Breakdown by GICS/ICB sector |
| Country exposure | Breakdown by country of listing/domicile |
| Currency exposure | Breakdown by currency of underlying positions |
| Cash weight | Cash as percentage of NAV |

### 8.2 Stress Testing (Simple)

- "What-if" scenarios: impact on fund NAV if market drops X%
- Sector-specific shocks: what if financials drop 15%?
- Currency shock: what if GBP strengthens 10% vs USD?
- Results displayed as estimated NAV impact in GBP and percentage

### 8.3 Risk Dashboards

- Firm-wide view: all funds with current exposure metrics, compliance status, and performance RAG rating
- Fund-level drill-down: full exposure breakdown with charts
- Alerts for funds approaching mandate limits (e.g., single stock at 9% with a 10% limit)

---

## 9. Operations

### 9.1 Settlement Tracking

- All executed trades tracked through settlement lifecycle
- Settlement dates calculated per market convention:
  - UK equities: T+1
  - US equities: T+1
  - European equities: T+2
  - Government bonds: T+1
- Dashboard showing: trades settling today, trades failing to settle, cash impact
- Failed settlement alerts and escalation

### 9.2 Corporate Actions

The system must handle corporate actions that affect positions:

| Action Type | Impact |
|-------------|--------|
| Cash dividend | Cash credited to fund, income recorded |
| Stock dividend | Additional shares added to position |
| Stock split / reverse split | Quantity adjusted, average cost recalculated |
| Rights issue | New shares offered, fund manager decides to take up or sell rights |
| Merger / acquisition | Position in old instrument converted to new instrument or cash |
| Delisting | Position marked, requires manual resolution |

**Processing:**
- Corporate actions entered manually by operations (or imported from data feed)
- Operations reviews and confirms the action details
- System calculates the impact on each affected fund's positions and cash
- Fund manager is notified of actions requiring decisions (e.g., rights issues)
- All processing is audited

### 9.3 NAV Calculation

End-of-day process to calculate official Net Asset Value per fund:

1. Pull official closing prices for all held instruments
2. Value each position: quantity x closing price
3. Convert non-base-currency positions using closing FX rates
4. Sum all position values + cash balances
5. Deduct accrued management fees and other liabilities
6. NAV = total assets - total liabilities
7. NAV per unit/share = NAV / units in issue

- NAV must be reviewed and approved by operations before publication
- Historical NAV series stored for performance calculation
- Discrepancies flagged if NAV move exceeds configurable threshold (e.g., > 3% daily move triggers review)

### 9.4 Reconciliation

- Daily reconciliation of internal positions against external custodian records
- Breaks (mismatches) flagged for operations investigation
- Break types: quantity mismatch, price mismatch, missing position, extra position
- Break resolution workflow with documented outcomes

---

## 10. UK Tax & Regulatory

### 10.1 Capital Gains Tax (CGT) Tracking

For taxable (non-ISA, non-pension) fund structures:

**HMRC Share Matching Rules (applied in order):**
1. **Same-day rule**: shares sold are matched first against shares bought on the same day
2. **Bed-and-breakfast rule (30 day)**: shares sold matched against shares bought within the following 30 days
3. **Section 104 pool**: remaining shares matched against the Section 104 holding (average cost pool)

- System maintains Section 104 pool per instrument per fund, updated on every trade
- Realised gains/losses calculated per disposal using the matching rules above
- Running total of realised gains per fund per tax year
- Annual CGT allowance tracking (where applicable)

### 10.2 Stamp Duty

- UK equity purchases subject to 0.5% Stamp Duty Reserve Tax (SDRT)
- Automatically calculated and included in trade cost
- Not applicable to: AIM stocks (from certain dates), ETFs, non-UK equities, gilts
- Stamp duty tracked per trade and reported in transaction cost analysis

### 10.3 Dividend Tax Treatment

- UK dividend income: no withholding for UK funds, but tracked for investor-level reporting
- Overseas dividend income: withholding tax rates vary by country and treaty
- Gross and net dividend amounts recorded
- Withholding tax reclaim tracking for applicable jurisdictions

### 10.4 FCA Reporting Considerations

- Transaction reporting data captured (MiFID-style fields even post-Brexit)
- Best execution data: execution venue, price, timing captured per trade
- Client money and asset segregation tracking per fund type
- Complaint and breach records maintained

---

## 11. Reporting

### 11.1 Standard Reports

| Report | Audience | Frequency |
|--------|----------|-----------|
| Fund Valuation | Fund Manager, Client | Daily |
| Holdings Report | Fund Manager, Compliance | Daily |
| Transaction Report | Operations, Compliance | Daily |
| Compliance Breach Report | Compliance, Risk | Daily |
| Performance Report (vs benchmark) | Fund Manager, Client | Daily / Monthly |
| Attribution Report | Fund Manager | Monthly |
| Risk Exposure Report | Risk Manager | Daily |
| Dealing Activity Report | Dealer, Compliance | Daily |
| NAV Report | Operations, Client | Daily |
| CGT Report | Operations, Tax | Quarterly / Annual |
| Trade Cost Analysis | Compliance, Fund Manager | Monthly |

### 11.2 Report Delivery

- All reports viewable in the UI with filtering and drill-down
- Export to PDF and CSV
- Scheduled email delivery for key reports (configurable per user)

---

## 12. Audit & Observability

### 12.1 Audit Trail

Every significant action in the system produces an audit event:

**Captured Fields:**
- Timestamp (UTC)
- User ID and role
- Action type (create, update, delete, approve, override, execute, etc.)
- Entity type and ID (fund, order, position, rule, etc.)
- Before and after state (for mutations)
- Reason/rationale (where applicable, e.g., compliance override reason)
- IP address and session ID

**Audit events are immutable** — they cannot be edited or deleted by any user including admins.

### 12.2 OpenTelemetry via .NET Aspire

Longstone uses **.NET Aspire** as the orchestration and telemetry backbone from day one. Aspire provides built-in OpenTelemetry integration, service discovery, and a developer dashboard for local development.

**Aspire Responsibilities:**
- Orchestrate all Longstone services and dependencies (database, background jobs, market data)
- Provide the Aspire Dashboard in development for real-time traces, logs, and metrics
- Wire up OpenTelemetry exporters to the production observability stack (OTEL Collector)
- Health checks and service readiness for all components

**Three Pillars — All Collected:**

**Traces:**
- End-to-end distributed tracing across HTTP requests, SignalR hubs, database queries, external market data API calls, and Hangfire background jobs
- Custom activity sources for domain operations: order lifecycle, compliance checks, NAV calculation
- Trace context propagated through SignalR messages and background job scheduling
- Exported via OTLP to OTEL Collector -> Tempo

**Metrics:**
- ASP.NET Core built-in metrics (request duration, active connections, error rates)
- Custom business metrics via System.Diagnostics.Metrics:
  - `longstone.orders.created` — counter of orders by fund/direction
  - `longstone.orders.fill_latency` — histogram of time from order to fill
  - `longstone.compliance.check_duration` — histogram of pre-trade check time
  - `longstone.compliance.breaches` — counter of breaches by rule type
  - `longstone.market_data.api_latency` — histogram of external API response time
  - `longstone.market_data.cache_hit_rate` — gauge of cache effectiveness
  - `longstone.market_data.stale_prices` — gauge of instruments with stale data
  - `longstone.nav.calculation_duration` — histogram per fund
  - `longstone.signalr.active_connections` — gauge by hub
- EF Core query metrics (duration, rows affected)
- Exported via OTLP to OTEL Collector -> Prometheus

**Logs:**
- Structured logging via `ILogger<T>` with automatic trace ID correlation
- Log levels: Information for business events, Warning for degraded states, Error for failures
- All logs include: TraceId, SpanId, UserId, FundId (where applicable)
- Sensitive data (passwords, tokens) never logged
- Exported via OTLP to OTEL Collector -> Loki

**Key Instrumented Paths:**
- Order lifecycle: trace from creation through compliance, execution, settlement
- Market data pipeline: external API call latency, cache hit rates, stale data events
- NAV calculation: per-fund calculation duration, pricing source breakdown
- Compliance engine: rule evaluation time, breach rates, override frequency
- SignalR hubs: connection lifecycle, message throughput, errors
- Hangfire jobs: execution duration, success/failure rates, queue depth

### 12.3 Alerting

- Compliance breach notifications (in-app + email)
- Market data feed interruption
- NAV movement exceeding threshold
- Settlement failure
- System health: API errors, background job failures, WebSocket disconnections

---

## 13. Non-Functional Requirements

### 13.1 Performance

| Metric | Target |
|--------|--------|
| API response time (p95) | < 200ms for reads, < 500ms for writes |
| Pre-trade compliance check | < 1 second per order |
| Dealer blotter update latency | < 500ms from fill to screen |
| EOD NAV calculation (per fund) | < 30 seconds |
| Concurrent users | 50+ simultaneous |

### 13.2 Availability

- Target uptime: 99.5% during market hours (08:00-16:30 GMT for LSE, extended for global markets)
- Graceful degradation: system remains usable with stale market data if feed drops
- Background jobs (NAV, compliance sweeps) are idempotent and retryable

### 13.3 Security

- HTTPS everywhere, TLS 1.3
- Authentication via OpenID Connect
- Role-based access control enforced at API level
- All sensitive fields encrypted at rest
- API rate limiting per user
- OWASP Top 10 mitigations
- Regular dependency vulnerability scanning

### 13.4 Data Retention

- Trade and order data: 7 years (FCA regulatory requirement)
- Audit logs: 7 years
- Market data: retained indefinitely (used for historical performance)
- User sessions: 90-day retention

---

## 14. Technical Architecture

### 14.1 Tech Stack

| Layer | Technology |
|-------|-----------|
| Orchestration | .NET Aspire (service orchestration, telemetry, health checks) |
| Frontend | Blazor Server (real-time via SignalR, server-rendered) |
| API | ASP.NET Core 9 Web API (backend services) |
| Database | SQLite (v1) — designed for easy swap to SQL Server |
| ORM | Entity Framework Core (code-first migrations) |
| Real-time | SignalR (dealer blotter, live prices, notifications) |
| Background Jobs | Hangfire |
| Market Data | Finnhub (WebSocket + REST) |
| Caching | In-memory + SQLite for market data |
| Observability | OpenTelemetry via .NET Aspire -> OTEL Collector -> Grafana/Tempo/Loki/Prometheus |
| Auth | OpenID Connect (Keycloak or similar) |
| CQRS/Messaging | MediatR |
| Deployment | Docker, Tailscale, UGREEN NAS |

### 14.2 Key Architecture Decisions

- **.NET Aspire from day one**: orchestrates services in development (Aspire Dashboard for traces/logs/metrics), configures OpenTelemetry exporters for production (OTLP -> OTEL Collector)
- **Blazor Server**: server-side rendering with built-in SignalR circuit — ideal for real-time dealer blotter and live portfolio updates without a separate API/SPA split
- **SQLite with SQL Server migration path**: EF Core abstracts the database provider. All data access via EF Core — no raw SQL, no provider-specific features. Connection string swap + migration re-run is the only change needed to move to SQL Server
- **CQRS**: separate read and write models for portfolio queries vs order commands
- **Event sourcing for orders**: full order lifecycle captured as events, state derived from event replay
- **Domain-driven design**: bounded contexts for Portfolio, Orders, Compliance, Market Data, Performance, Operations
- **Background job scheduling**: EOD NAV, compliance sweeps, market data pulls, settlement checks
- **SignalR hubs**: separate hubs for dealer blotter, portfolio updates, notifications

### 14.3 Database Strategy

**SQLite (v1):**
- Zero infrastructure — single file, no server process
- Ideal for development, testing, and NAS deployment
- EF Core SQLite provider with code-first migrations
- Database file stored in a Docker volume for persistence
- WAL mode enabled for concurrent read performance

**SQL Server (future):**
- Swap `UseSqlite()` for `UseSqlServer()` in DI registration
- Generate new migration set for SQL Server provider
- No application code changes required
- Enables: better concurrency, full-text search, row-level security, larger scale

**Design constraints to ensure clean swap:**
- No SQLite-specific SQL or pragmas in application code
- No provider-specific EF Core features (e.g., no SQLite collation hacks)
- All queries go through EF Core LINQ — no `FromSqlRaw()` unless provider-agnostic
- Integration tests run against both SQLite and SQL Server (when SQL Server target is added)

### 14.4 .NET Aspire Architecture

```
Longstone.AppHost/                  -- Aspire orchestrator (Program.cs)
  |
  |-- Longstone.Web                 -- Blazor Server app (main UI + API)
  |-- Longstone.ServiceDefaults/    -- Shared Aspire config (OpenTelemetry, health checks, resilience)
  |-- SQLite database               -- Referenced as a resource
  |-- Hangfire                      -- Background job server
  |-- Finnhub WebSocket             -- Market data connection
```

**Development:** Aspire Dashboard provides real-time traces, logs, and metrics at `https://localhost:15888`. No external observability stack needed during development.

**Production (NAS):** Aspire's `ServiceDefaults` configures OTLP export to the OTEL Collector running on the NAS. The Aspire AppHost is not deployed — only the configured services run in Docker.

### 14.5 Project Structure (Feature-Based)

```
src/
  Longstone.AppHost/             -- .NET Aspire orchestrator
  Longstone.ServiceDefaults/     -- Shared Aspire defaults (OpenTelemetry, health checks)
  Longstone.Web/                 -- Blazor Server app (UI + API controllers)
  Longstone.Domain/              -- Core domain entities, value objects, interfaces
  Longstone.Application/         -- Use cases, commands, queries, handlers (MediatR)
  Longstone.Infrastructure/      -- EF Core, external APIs, SignalR hubs, Hangfire jobs
  Longstone.Compliance/          -- Compliance engine, rule evaluation, mandate checks
  Longstone.MarketData/          -- Market data integration, caching, WebSocket client
  Longstone.Performance/         -- TWR calculation, attribution, benchmarking
tests/
  Longstone.Domain.Tests/
  Longstone.Application.Tests/
  Longstone.Compliance.Tests/
  Longstone.Performance.Tests/
  Longstone.Integration.Tests/
```

---

## 15. Deployment & Infrastructure

### 15.1 Target Environment

Longstone deploys to a **UGREEN NAS** running Docker, following the same proven pattern used by MoonlightMayhem.

**NAS Prerequisites:**
- Docker installed and running
- SSH access enabled
- Minimum 1GB RAM available (app + database)
- Network access from LAN clients
- Tailscale auth key for remote access
- Observability stack already running (shared with other apps on the NAS)

### 15.2 Container Architecture

```
┌─────────────────────────────────────────────────────┐
│ docker-compose.hub.yml (Longstone)                  │
│                                                     │
│  ┌─────────────┐    ┌──────────────────────────┐    │
│  │  Tailscale   │◄───│  Longstone.Web            │    │
│  │  (HTTPS +    │    │  (Blazor Server)          │    │
│  │   Funnel)    │    │  Port 8080                │    │
│  └─────────────┘    │  Volume: longstone-data    │    │
│                      │  (SQLite DB + data files)  │    │
│                      └──────────────────────────┘    │
└─────────────────────────────────────────────────────┘
        │
        │ OTLP (gRPC :4317)
        ▼
┌─────────────────────────────────────────────────────┐
│ observability/ (shared, already running on NAS)      │
│                                                     │
│  OTEL Collector → Tempo (traces)                    │
│                 → Loki (logs)                       │
│                 → Prometheus (metrics)              │
│  Grafana (dashboards) :3000                         │
└─────────────────────────────────────────────────────┘
```

### 15.3 Docker Configuration

**Dockerfile** — multi-stage build, Alpine runtime for minimal image size:
- Build stage: `mcr.microsoft.com/dotnet/sdk:9.0-alpine`
- Runtime stage: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine`
- Target image size: < 200MB
- Exposes port 8080
- SQLite database stored in `/app/data` (Docker volume)

**docker-compose.hub.yml** — production deployment:
- **Tailscale service**: provides HTTPS via Tailscale Funnel, custom domain support
- **Longstone app**: Blazor Server running on port 8080, network mode through Tailscale
- **Volumes**: `longstone-data` for SQLite database persistence, `tailscale-state` for VPN state
- **Network**: connects to `observability-net` (external) to reach the shared OTEL Collector
- **Health check**: `http://localhost:8080/health` every 30s
- **Environment**: OTLP endpoint pointing to NAS observability stack, production ASP.NET Core config

**tailscale-serve.json** — reverse proxy configuration:
- Maps Tailscale domain (`longstone.tail701687.ts.net:443`) to `http://127.0.0.1:8080`
- Maps custom domain (e.g., `longstone.gavindraper.com:443`) to `http://127.0.0.1:8080`
- AllowFunnel enabled for external access

### 15.4 Single-Script Deployment

All deployment is handled by a single script: `scripts/deploy-to-nas.sh`

```bash
./scripts/deploy-to-nas.sh
```

**What the script does:**
1. Builds the Docker image locally with a version tag
2. Saves the image to a tar file
3. SCPs the tar to the NAS
4. SSHs into the NAS and loads the image
5. Stops the old container
6. Starts the new container (via docker-compose)
7. Removes the old image to reclaim space
8. Waits for health check to confirm the app is running
9. Outputs the access URL

**Script configuration** (via `.env` or script variables):
- `NAS_HOST` — NAS IP or hostname (e.g., `192.168.184.108`)
- `NAS_USER` — SSH user on the NAS
- `NAS_DEPLOY_PATH` — path on NAS where compose files live (e.g., `/opt/longstone`)
- `IMAGE_NAME` — Docker image name (e.g., `longstone`)

**Rollback:** previous image tags are preserved. To rollback, SSH into NAS and `docker-compose up -d` with the previous tag.

### 15.5 Observability Stack (Shared)

The observability stack is **already deployed on the NAS** and shared across applications. Longstone connects to it via the `observability-net` Docker network.

**Stack Components (already running):**

| Service | Port | Purpose |
|---------|------|---------|
| OTEL Collector | 4317 (gRPC), 4318 (HTTP) | Receives OTLP data, routes to backends |
| Tempo | 3200 | Distributed tracing (72h retention) |
| Loki | 3100 | Log aggregation (72h retention) |
| Prometheus | 9090 | Metrics (1GB storage limit) |
| Grafana | 3000 | Dashboards for all three signals |

**Longstone-specific Grafana dashboards (to be created):**
- Order lifecycle: throughput, latency, compliance check pass/fail rates
- Market data: API latency, cache hit rates, stale data counts
- System health: request rates, error rates, active SignalR connections
- Business: daily order volume, NAV calculation times, breach frequency

### 15.6 Environment Configuration

**.env.example:**
```
# Tailscale
TS_AUTHKEY=tskey-auth-xxxxx

# OpenTelemetry
OTEL_EXPORTER_OTLP_ENDPOINT=http://192.168.184.108:4317
OTEL_SERVICE_NAME=Longstone

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production

# Database (SQLite path inside container)
DATA_PATH=/app/data
```

### 15.7 Local Development

In development, **.NET Aspire** handles everything:
- `dotnet run --project src/Longstone.AppHost` starts all services
- Aspire Dashboard at `https://localhost:15888` shows traces, logs, and metrics
- No Docker, no OTEL Collector, no Grafana needed locally
- SQLite database file created in the project's local data directory
- Hot reload supported for Blazor Server

---

## 16. Phased Delivery

### Phase 1: Foundation (Weeks 1-2)
- .NET Aspire solution structure (`AppHost`, `ServiceDefaults`, `Web`, `Domain`, `Infrastructure`)
- OpenTelemetry wired up via Aspire ServiceDefaults (traces, metrics, logs exporting via OTLP)
- SQLite database with EF Core, code-first migrations
- Instrument reference data model and seeding
- Fund CRUD with basic properties
- User management and RBAC
- Market data integration (REST — end-of-day prices)
- Basic Blazor Server UI shell with navigation and auth
- Dockerfile and docker-compose.hub.yml for NAS deployment
- `scripts/deploy-to-nas.sh` — single-command deployment working end-to-end
- Health check endpoint

### Phase 2: Portfolio & Orders (Weeks 3-4)
- Holdings and position management
- Cash accounts and balances
- Order instruction creation (fund manager flow)
- Pre-trade compliance engine (core rules)
- Order lifecycle state machine
- Dealer blotter with SignalR real-time updates
- Trade execution recording
- Position updates on settlement

### Phase 3: Compliance & Risk (Weeks 5-6)
- Full mandate rule configuration UI
- Post-trade compliance monitoring (scheduled via Hangfire)
- Banned and restricted instrument lists
- Compliance breach workflow (override/reject)
- Exposure analysis dashboards (sector, country, currency)
- Stress testing (simple what-if scenarios)
- Risk alerting

### Phase 4: Performance & Operations (Weeks 7-8)
- NAV calculation engine (EOD Hangfire job)
- Time-weighted return calculation
- Benchmark comparison and tracking error
- Basic Brinson attribution (sector level)
- Corporate actions processing
- Settlement tracking dashboard
- Reconciliation framework

### Phase 5: Tax, Reporting & Polish (Weeks 9-10)
- Section 104 pool maintenance and CGT calculation
- Stamp duty tracking
- Dividend and withholding tax tracking
- Standard report generation (PDF/CSV)
- Model portfolios and rebalancing proposals
- Block order allocation
- Grafana dashboards for Longstone business metrics
- Performance and UX polish

---

## 17. Out of Scope (v1)

The following are explicitly not included in the initial build:

- Real money trading or broker integration (execution is simulated/manual)
- Multi-currency accounting (simplified FX conversion only)
- Derivatives, options, or complex instruments
- Automated trading or algorithmic execution
- Client portal (external-facing)
- Mobile application
- Full MiFID II transaction reporting
- Real-time P&L (end-of-day is sufficient for v1)
- Hedging or FX overlay

---

## 18. Success Criteria

The system is considered complete when:

1. A fund manager can create a fund, define its mandate, and construct a portfolio
2. Orders flow through the full lifecycle: creation -> compliance -> dealing -> settlement
3. Pre-trade compliance blocks orders that breach mandate rules
4. A dealer can manage a real-time blotter and record executions
5. End-of-day NAV is calculated correctly for all active funds
6. Performance is calculated using TWR and compared against benchmarks
7. UK tax rules (Section 104, stamp duty) are applied correctly
8. All state changes produce audit events
9. OpenTelemetry traces, metrics, and logs flow from the app through OTEL Collector to Grafana/Tempo/Loki/Prometheus
10. `./scripts/deploy-to-nas.sh` deploys the entire application to the NAS in a single command
11. The app is accessible via Tailscale Funnel with HTTPS on a custom domain
12. The system handles 50 concurrent users without degradation
