# Performance Baseline — Syrx.SqlServer 3.0.0

This document records the baseline performance measurements for Syrx.SqlServer 3.0.0 on .NET 10.0.

---

## Methodology

Benchmarks are executed using BenchmarkDotNet in Release configuration against a local SQL Server 2022 container (Docker).

```bash
cd tests/performance/Syrx.SqlServer.Tests.Performance
dotnet run -c Release
```

Each benchmark runs a minimum of 15 iterations with warm-up passes. Results capture:

- **Mean** latency (μs)
- **p95 / p99** latency percentiles
- **Allocated bytes** per operation
- **Gen0 GC** collections per 1000 operations

---

## Environment

> Update this section when baselines are recorded. Example format:

| Property | Value |
|----------|-------|
| Date | _TBD — run benchmarks and record_ |
| OS | (e.g., Windows 11 23H2 / Ubuntu 22.04) |
| CPU | (e.g., Intel Core i9-13900K) |
| RAM | (e.g., 32 GB DDR5) |
| .NET Runtime | net10.0.x |
| SQL Server | 2022 (Docker), `mcr.microsoft.com/mssql/server:2022-latest` |
| Connection pool | Min=5, Max=100 |

---

## Baseline Measurements

> Run the benchmark suite and paste results below. Example structure:

### Single-row query (SELECT by ID)

| Method | Mean | p95 | p99 | Allocated |
|--------|------|-----|-----|-----------|
| Syrx (cold) | — | — | — | — |
| Syrx (warm) | — | — | — | — |
| ADO.NET baseline | — | — | — | — |

### Multi-row query (SELECT top 100)

| Method | Mean | p95 | p99 | Allocated |
|--------|------|-----|-----|-----------|
| Syrx | — | — | — | — |
| ADO.NET baseline | — | — | — | — |

### Execute (INSERT with transaction)

| Method | Mean | p95 | p99 | Allocated |
|--------|------|-----|-----|-----------|
| Syrx | — | — | — | — |
| ADO.NET baseline | — | — | — | — |

---

## Performance Targets (SLA)

Based on findings in the performance research report, the following targets define acceptable performance for production workloads:

| Scenario | p50 target | p99 target | Notes |
|----------|-----------|-----------|-------|
| Single-row SELECT | < 2 ms | < 10 ms | Excludes network latency to remote DB |
| Multi-row SELECT (100) | < 5 ms | < 20 ms | |
| INSERT + transaction | < 5 ms | < 25 ms | Includes transaction commit |

Regressions beyond ±10% of these targets trigger investigation before release.

---

## Recording Results

Run the full benchmark suite and paste BenchmarkDotNet summary output here:

```
# Paste full BenchmarkDotNet markdown table output here
```

---

## Next Steps

- [ ] Run benchmark suite on representative hardware
- [ ] Record results in the tables above
- [ ] Confirm all scenarios meet SLA targets
- [ ] Commit with message: `perf: record 3.0.0 production baselines`

---

*Last updated: 2026-03-21 — baselines pending first benchmark run*
