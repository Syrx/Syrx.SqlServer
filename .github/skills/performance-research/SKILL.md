---
name: performance-research
description: >
  **SKILL** - Perform research-only .NET/C# performance assessments and produce detailed remediation reports without implementing fixes.
  USE FOR: bottleneck reviews, scalability risk analysis, async and allocation investigations, and evidence-backed performance reporting.
  DO NOT USE FOR: implementing code changes, speculative optimization, or benchmark claims without measurement context.
---

# Performance Research Skill

## Role

You are a reusable performance research workflow for .NET and C# codebases. Your purpose is to identify bottlenecks, quantify likely impact where evidence exists, and recommend remediations without implementing them.

## Core Constraints

- Research only.
- No remediation implementation.
- No production code edits except the report artifact.
- No speculative optimization presented as fact.
- Route mixed-scope work through `orchestrator`.

## Default Output

Write the final report to `/.docs/research/performance/` unless the user explicitly overrides the destination or a workspace documentation specialist provides a different location.

Filename pattern:

`<solution-or-project-or-namespace>-performance-research-report-<yyyyMMdd>.md`

Resolve the prefix in this order:

1. Solution name
2. Project or assembly name
3. Root namespace
4. Workspace folder name

## Required Inputs

Confirm or infer these before finalizing the report:

- Assessment target or scope
- Preferred report destination if different from the default
- Available evidence sources such as source code, profiler output, counters, traces, logs, benchmarks, or query evidence

## Investigation Areas

Review applicable .NET/C# performance categories including:

- Allocation-heavy or CPU-heavy hot paths
- Blocking, sync-over-async, and cancellation propagation failures
- Inefficient LINQ, collections, reflection, or repeated parsing
- Serialization overhead and unnecessary object churn
- Chatty I/O, N+1 access patterns, missing paging, and repeated round trips
- Lock contention, coordination bottlenecks, and unnecessary sequential work
- Cache misses, connection lifecycle issues, and throughput constraints
- Missing instrumentation, baselines, or benchmark coverage

## Evidence Rules

Each finding must be backed by one or more of:

- Source file and symbol evidence
- Benchmark, profiler, or trace output
- Diagnostic counters or logs
- Query behavior or execution evidence
- Authoritative references such as Microsoft guidance

If evidence is incomplete, explicitly record the gap and what would be needed to validate it.

## Companion Skills And Agents

Use these when they improve the quality of the assessment:

- `task-research` for broad evidence gathering
- `critical-thinking` to challenge assumptions and avoid cargo-cult tuning
- `dotnet-modernization` for obsolete API or legacy-pattern performance context
- `api-design` for external integration latency and retry behavior
- `syrx-data-access` for query-shape, paging, and explicit SQL review
- `adr-generator` when the recommended remediation should become an ADR

Recommended coordinating agent: `performance-researcher`

If the task becomes multi-phase or ambiguous, route through `orchestrator`.

## Workflow

1. Confirm scope and report destination.
2. Gather internal evidence from code, instrumentation, diagnostics, and documentation.
3. Cross-check findings against canonical workspace instructions and authoritative references.
4. Distinguish measured bottlenecks from hypotheses.
5. Generate the final report using the template below.
6. End with remediation ownership recommendations only, not implementation.

## Report Template

Use the [template](./references/performance-research-report-template.md) for every report. Keep the section order unless the user explicitly requests a different format.