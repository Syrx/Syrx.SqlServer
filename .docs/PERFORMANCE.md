# Performance Guide

This document describes performance characteristics, instrumentation options, and tuning guidance for Syrx.SqlServer.

---

## Architecture Overview

```
Repository (ICommander<T>)
    ↓ Dapper + parameterized SQL
SqlServerDatabaseConnector
    ↓ Microsoft.Data.SqlClient connection pool
SQL Server
```

- **Connection pooling** is handled automatically by `Microsoft.Data.SqlClient`. The pool is keyed by connection string, so identical strings share pool instances.
- **Command settings** (`ICommanderSettings`) are cached on a `ConcurrentDictionary` after first resolution, eliminating repeated LINQ lookups on hot paths.
- **No reflection** occurs at query execution time — Dapper uses pre-compiled expression trees after the initial invocation per type.

---

## Connection Pooling

### Recommended connection string settings

```
Server=myserver;Database=Syrx;Integrated Security=true;
Min Pool Size=5;Max Pool Size=100;
Connection Timeout=15;Command Timeout=30;
```

| Setting | Recommendation | Notes |
|---------|----------------|-------|
| `Min Pool Size` | 5 | Avoids cold-start latency on burst traffic |
| `Max Pool Size` | 50–200 | Size to peak concurrent workload |
| `Connection Timeout` | 15s | Fail fast under saturation |
| `Command Timeout` | 30s | Per-command SLA; override per method if needed |

### Read/write separation

Use two connection aliases to route reads to a replica:

```csharp
services.UseSyrx(b => b
    .UseSqlServer(s => s
        .AddConnectionString("ReadOnly", readReplicaConnectionString)
        .AddConnectionString("Primary", primaryConnectionString)
        .AddCommand(types => types
            .ForType<MyRepository>(methods => methods
                .ForMethod("GetAll", cmd => cmd.UseConnectionAlias("ReadOnly")
                    .UseCommandText("SELECT ..."))
                .ForMethod("Create", cmd => cmd.UseConnectionAlias("Primary")
                    .UseCommandText("INSERT..."))))));
```

---

## Instrumentation

### OpenTelemetry (recommended)

`Microsoft.Data.SqlClient` emits built-in OpenTelemetry traces for all database operations when the `SqlClientInstrumentation` package is referenced.

```bash
dotnet add package OpenTelemetry.Instrumentation.SqlClient
dotnet add package OpenTelemetry.Exporter.Console     # development
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol  # production
```

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;   // include SQL in spans (dev only)
            options.RecordException = true;
        })
        .AddConsoleExporter());                     // swap for OTLP in production
```

> ⚠️ Never enable `SetDbStatementForText = true` in production without reviewing what SQL is captured — it may include parameter values in some configurations.

### Available metrics

When `SqlClientInstrumentation` is enabled, the following spans are emitted per query:

| Span attribute | Value |
|---|---|
| `db.system` | `mssql` |
| `db.name` | database name |
| `db.statement` | SQL text (if enabled) |
| `db.operation` | statement type (`SELECT`, `INSERT`, etc.) |
| `net.peer.name` | server hostname |
| `net.peer.port` | port |

---

## Benchmark Suite

The `tests/performance/Syrx.SqlServer.Tests.Performance/` project contains a BenchmarkDotNet suite.

### Running benchmarks

```bash
cd tests/performance/Syrx.SqlServer.Tests.Performance
dotnet run -c Release
```

Requires a running SQL Server instance (see `Docker/` folder for setup).

### Interpreting results

Results are compared against a direct ADO.NET baseline in the same benchmark class. Syrx overhead is expected to be <5μs per query (configuration cache hit after warm-up).

---

## Known Performance Characteristics

| Scenario | Typical overhead | Notes |
|----------|-----------------|-------|
| First query (cold cache) | ~200–500μs extra | One-time LINQ settings lookup + cache population |
| Subsequent queries (warm cache) | <5μs extra | Direct `ConcurrentDictionary` lookup |
| Multi-mapping queries | Same as Dapper raw | No additional allocation |
| `ExecuteAsync` (write + transaction) | Connection + transaction overhead | Expected; isolation level configurable |

---

## Performance Findings

The full performance research report is at [`.docs/research/performance/`](./research/performance/).

Key actionable findings:

1. **PERF-005** *(Critical)*: No production telemetry — address by adding `OpenTelemetry.Instrumentation.SqlClient` as described above.
2. **PERF-006** *(High)*: No documented production baselines — run benchmarks under representative load and record p50/p95/p99 in `.docs/PERFORMANCE-BASELINE.md`.
3. **PERF-002** *(Medium)*: Initial cold-cache LINQ lookup cost — acceptable at current load; monitor under burst concurrency.

---

## Tuning Checklist

- [ ] Connection pool sized to peak concurrent workload (`Max Pool Size`)
- [ ] Read/write separation configured where replicas are available
- [ ] OpenTelemetry instrumentation added and exporter configured
- [ ] `SetDbStatementForText` disabled in production
- [ ] Benchmark suite has been run and baseline documented
- [ ] `Command Timeout` set per method for long-running queries

---

*Last updated: 2026-03-21*
