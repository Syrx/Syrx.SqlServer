---
name: security-research
description: 'Prompt for research-only .NET/C# security assessment with a detailed remediation report and no implementation.'
---

# Security Research Prompt

Route this request to `security-researcher`.

Perform a research-only security assessment of the requested .NET/C# scope. Use the `security-research` skill and relevant companion skills to gather evidence, identify vulnerabilities, and write a detailed remediation report without implementing fixes.

Default output:

- Report location: `/.docs/research/security/`
- Filename: `<solution-or-project-or-namespace>-security-research-report-<yyyyMMdd>.md`

Requirements:

- Collaborate through `orchestrator` if the task spans multiple agents.
- Use existing workspace instructions and relevant skills.
- Call out missing skills, missing evidence, or missing tooling in the report.
- Keep implementation status explicit: no remediations are to be implemented.