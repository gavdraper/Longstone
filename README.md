# Longstone

A sandbox project to explore what Claude Opus 4.6 can build in a short space of time.

## The Scenario

Longstone is a **portfolio management system** for UK-based investment management firms. It covers the full investment lifecycle: fund setup, order management, compliance checks, dealing, settlement, NAV calculation, performance measurement, and regulatory reporting — all under UK (FCA/HMRC) rules.

Target users include fund managers, dealers, compliance officers, operations staff, and risk managers.

## Key Capabilities

- **Fund & Portfolio Management** — create funds, define mandates, manage holdings and cash
- **Order Management System** — full order lifecycle from draft through compliance, execution, and settlement
- **Compliance Engine** — pre-trade and post-trade mandate checks with breach workflows
- **Dealer Blotter** — real-time order management via SignalR
- **Performance & Attribution** — time-weighted returns, benchmark comparison, Brinson attribution
- **Risk Management** — exposure analysis, concentration monitoring, stress testing
- **Operations** — NAV calculation, corporate actions, settlement tracking, reconciliation
- **UK Tax** — Section 104 pooling, stamp duty, dividend withholding tax
- **Audit Trail** — immutable event log for every state change
- **Observability** — full OpenTelemetry (traces, metrics, logs) via .NET Aspire

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Orchestration | .NET Aspire |
| Frontend | Blazor Server |
| API | ASP.NET Core 9 |
| Database | SQLite (v1), designed for SQL Server swap |
| ORM | Entity Framework Core |
| Real-time | SignalR |
| Background Jobs | Hangfire |
| Market Data | Finnhub (WebSocket + REST) |
| Observability | OpenTelemetry via Aspire -> OTEL Collector -> Grafana/Tempo/Loki/Prometheus |
| CQRS | MediatR |
| Deployment | Docker, Tailscale, UGREEN NAS |

## Running Locally

```bash
dotnet run --project src/Longstone.AppHost
```

Aspire Dashboard available at `https://localhost:15888`.

## Deploying

```bash
./scripts/deploy-to-nas.sh
```

See [PRD.md](PRD.md) for full requirements and architecture details.
