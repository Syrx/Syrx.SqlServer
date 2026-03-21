# Security research report

## Metadata

- Report name: `<solution-or-project-or-namespace>-security-research-report-<yyyyMMdd>.md`
- Generated on: `<yyyy-MM-dd>`
- Assessor: `security-researcher`
- Scope: `<solution/project/namespace/component>`
- Report location: `/.docs/research/security/`
- Evidence sources: `<code, analyzers, logs, docs, dependency manifests, runtime output>`

## Scope and constraints

- In scope: `<explicit components reviewed>`
- Out of scope: `<explicit exclusions>`
- Constraints: `<missing access, missing telemetry, no runtime access, no analyzer output, etc.>`

## Methodology

- Workspace instructions reviewed: `<relevant instruction files>`
- Skills used: `<skills actually used>`
- Agents consulted through orchestrator: `<agents actually used>`
- Research methods: `<searches, file inspection, diagnostics, references>`

## Executive summary

Provide a concise summary of the overall security posture, the highest-risk findings, and whether missing information materially limits confidence.

## Findings summary

| ID | Severity | Confidence | Category | Affected area | Short title |
|---|---|---|---|---|---|
| SEC-001 | High | High | Injection | `<component>` | `<title>` |

## Detailed findings

### SEC-001 `<title>`

- Severity: `<Critical/High/Medium/Low>`
- Confidence: `<High/Medium/Low>`
- Category: `<Injection/Auth/Secrets/Serialization/etc.>`
- CWE: `<CWE id or N/A>`
- OWASP: `<OWASP category or N/A>`
- Affected files or symbols: `<files, types, methods, endpoints>`
- Evidence:
  - `<concrete evidence item>`
  - `<concrete evidence item>`
- Impact:
  - `<impact and exploitability>`
- Recommended remediation:
  - `<clear remediation guidance without implementation>`
- Recommended validating agent or skill:
  - `<csharp-engineering / architecture-and-ddd / api-design / syrx-data-access / etc.>`
- Implementation status:
  - `Not implemented by security-researcher`

Repeat the section above for each finding.

## Missing skills, information, or tooling

- Missing skill coverage: `<if none, state none>`
- Missing evidence: `<logs, runtime access, dependency reports, environment details, etc.>`
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