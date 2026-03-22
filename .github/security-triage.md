# Security Triage Ownership

This repository enforces blocking security gates in CI:

- Dependency review: block on severity `high` and `critical`.
- Secret scanning: block on any detected secret.
- SAST: run CodeQL `security-extended` and `security-and-quality` suites on pull requests and main branch pushes.

## Triage Process

1. The pull request author is the first responder for failed security checks.
2. The maintainer on release duty is the escalation owner when the author cannot resolve findings.
3. Findings must include a tracked decision before merge: fix, accepted risk with expiry date, or false positive rationale.
4. Any accepted risk requires an issue link and a due date.

## SLA

- Critical and high findings: resolve before merge.
- Medium findings: resolve or document approved risk before release.
- Low findings: backlog issue required before merge.
