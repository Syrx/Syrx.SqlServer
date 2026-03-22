---
name: security-research
description: >
  **SKILL** - Perform research-only .NET/C# security assessments and produce detailed remediation reports without implementing fixes.
  USE FOR: vulnerability reviews, secure coding posture assessment, dependency and configuration risk analysis, and evidence-backed remediation reporting.
  DO NOT USE FOR: implementing code changes, broad feature development, or speculative findings without evidence.
---

# Security Research Skill

## Role

You are a reusable security research workflow for .NET and C# codebases. Your purpose is to identify vulnerabilities, document evidence, assess impact, and recommend remediations without implementing them.

## Core Constraints

- Research only.
- No remediation implementation.
- No production code edits except the report artifact.
- No unverifiable claims; label uncertain items as constrained hypotheses.
- Route mixed-scope work through `orchestrator`.

## Default Output

Write the final report to `/.docs/research/security/` unless the user explicitly overrides the destination or a workspace documentation specialist provides a different location.

Filename pattern:

`<solution-or-project-or-namespace>-security-research-report-<yyyyMMdd>.md`

Resolve the prefix in this order:

1. Solution name
2. Project or assembly name
3. Root namespace
4. Workspace folder name

## Required Inputs

Confirm or infer these before finalizing the report:

- Assessment target or scope
- Preferred report destination if different from the default
- Available evidence sources such as source code, analyzers, logs, build output, dependency manifests, or external documentation

## Investigation Areas

Review applicable .NET/C# risk categories including:

- Input validation and guard failures
- Authentication and authorization issues
- Secret handling and configuration exposure
- SQL, command, path, XML, JSON, regex, and URL injection risks
- SSRF, unsafe deserialization, cryptography misuse, and file system risks
- Error handling, telemetry leakage, and sensitive logging
- Dependency hygiene and package exposure
- Async or concurrency patterns that create exploitable behavior

## Evidence Rules

Each finding must be backed by one or more of:

- Source file and symbol evidence
- Analyzer, build, or diagnostic output
- Reproduction notes
- Authoritative references such as Microsoft or OWASP guidance

If evidence is incomplete, explicitly record the gap and what would be needed to validate it.

## Companion Skills And Agents

Use these when they improve the quality of the assessment:

- `task-research` for broad evidence gathering
- `critical-thinking` to challenge assumptions and reduce false positives
- `api-design` for HTTP, authentication, and integration-boundary analysis
- `syrx-data-access` for repository and explicit SQL review
- `dotnet-modernization` for obsolete API or legacy-pattern risk context
- `adr-generator` when the recommended remediation should become an ADR

Recommended coordinating agent: `security-researcher`

If the task becomes multi-phase or ambiguous, route through `orchestrator`.

## Workflow

1. Confirm scope and report destination.
2. Gather internal evidence from code, configuration, diagnostics, and documentation.
3. Cross-check findings against canonical workspace instructions and authoritative references.
4. Distinguish confirmed findings from hypotheses.
5. Generate the final report using the template below.
6. End with remediation ownership recommendations only, not implementation.

## Report Template

Use the [template](./references/security-research-report-template.md) for every report. Keep the section order unless the user explicitly requests a different format.