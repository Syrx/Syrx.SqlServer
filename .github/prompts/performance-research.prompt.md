---
name: performance-research
description: 'Prompt for research-only .NET/C# performance assessment with a detailed remediation report and no implementation.'
---

# Performance Research Prompt

Route this request to `performance-researcher`.

Perform a research-only performance assessment of the requested .NET/C# scope. Use the `performance-research` skill and relevant companion skills to gather evidence, identify bottlenecks, and write a detailed remediation report without implementing fixes.

Default output:

- Report location: `/.docs/research/performance/`
- Filename: `<solution-or-project-or-namespace>-performance-research-report-<yyyyMMdd>.md`

Requirements:

- Collaborate through `orchestrator` if the task spans multiple agents.
- Use existing workspace instructions and relevant skills.
- Call out missing skills, missing evidence, or missing tooling in the report.
- Keep implementation status explicit: no remediations are to be implemented.