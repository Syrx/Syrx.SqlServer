# Performance research report

## Metadata

- Report name: `<solution-or-project-or-namespace>-performance-research-report-<yyyyMMdd>.md`
- Generated on: `<yyyy-MM-dd>`
- Assessor: `performance-researcher`
- Scope: `<solution/project/namespace/component>`
- Report location: `/.docs/research/performance/`
- Evidence sources: `<code, profiler output, traces, counters, benchmarks, query evidence, logs>`

## Scope and constraints

- In scope: `<explicit components reviewed>`
- Out of scope: `<explicit exclusions>`
- Constraints: `<missing profiler data, no production telemetry, no database plan access, etc.>`

## Methodology

- Workspace instructions reviewed: `<relevant instruction files>`
- Skills used: `<skills actually used>`
- Agents consulted through orchestrator: `<agents actually used>`
- Research methods: `<searches, file inspection, traces, counters, benchmarks, references>`

## Executive summary

Provide a concise summary of the dominant bottlenecks, the likely business or operational impact, and whether limited instrumentation affects confidence.

## Findings summary

| ID | Priority | Confidence | Category | Affected area | Short title |
|---|---|---|---|---|---|
| PERF-001 | High | High | Async | `<component>` | `<title>` |

## Detailed findings

### PERF-001 `<title>`

- Priority: `<Critical/High/Medium/Low>`
- Confidence: `<High/Medium/Low>`
- Category: `<Async/Allocation/I-O/Query/Locking/etc.>`
- Affected files or symbols: `<files, types, methods, jobs, endpoints>`
- Evidence:
  - `<concrete evidence item>`
  - `<concrete evidence item>`
- Impact:
  - `<latency, throughput, allocation, or scalability discussion>`
- Recommended remediation:
  - `<clear remediation guidance without implementation>`
- Recommended validating agent or skill:
  - `<csharp-engineering / architecture-and-ddd / syrx-data-access / etc.>`
- Validation or benchmark recommendation:
  - `<specific benchmark, counter, trace, or load-test follow-up>`
- Implementation status:
  - `Not implemented by performance-researcher`

Repeat the section above for each finding.

## Missing skills, information, instrumentation, or tooling

- Missing skill coverage: `<if none, state none>`
- Missing evidence: `<benchmarks, production counters, database plans, representative load, etc.>`
- Confidence impact: `<how the missing inputs affect the assessment>`

## Cross-agent remediation handoff recommendations

| Recommendation | Owner | Why |
|---|---|---|
| `<recommendation>` | `csharp-engineering` | `<reason>` |

## Appendix

### Files and symbols reviewed

- `<file or symbol>`

### Searches and diagnostics used

- `<tool and query>`

### References

- `<authoritative reference>`

### Assumptions

- `<explicit assumption>`