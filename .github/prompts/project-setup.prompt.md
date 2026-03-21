---
name: project-setup
description: 'Prompt for scaffolding lean .NET project workspace configuration and copilot instructions.'
---

# Project Setup Workflow

Your goal is to create tailored workspace configuration for a new lean .NET project. Gather deep context via a questionnaire, audit the workspace for canonical patterns, curate agent and skill recommendations, and generate a complete project initialization package.

## Pre-Flight

Confirm these inputs before starting:
- `project_name` — Name of the project (e.g., AcmeInventory.API)
- `project_type` — REST API, Console Tool, Background Service, etc.
- `target_framework` — .NET version (e.g., .NET 9, .NET 8 LTS)

Request any missing inputs before proceeding.

## Questionnaire (12 Questions)

**Phase 1: Goals & Domain**
1. Problem statement and business goal — what success looks like
2. Core aggregates and domain entities — key invariants and relationships
3. External integrations — payment, queues, APIs, other services

**Phase 2: Architecture & Data**
4. Data model — relational, document, hybrid? Any auditing or temporal needs?
5. Concurrency and scale assumptions — throughput, batch, latency targets?
6. Boundary strategy — monolith, layered, services? Domain events or sagas?

**Phase 3: Security & Compliance**
7. Auth/authz model — internal, multi-tenant, customer-facing, role vs. permission-based?
8. Data sensitivity — PII, financial, regulated? Audit or retention requirements?

**Phase 4: Tooling & Constraints**
9. Team expertise — experience with Syrx, Moq, xUnit, ADR discipline?
10. Performance SLAs — P99 latency, throughput, error rate, availability targets?
11. Deployment strategy — Azure App Service, Kubernetes, on-premises? Secrets management?

**Phase 5: Scope**
12. Explicit non-goals — what is NOT in Phase 1? Where is the boundary?

**Key:** Follow up on vague answers. Do not proceed to analysis until all answers are concrete.

## Analysis

1. **Audit workspace** — extract Syrx patterns, guard semantics, test naming, documentation structure from existing projects
2. **Curate agents** — match project needs to: `csharp-engineering`, `architecture-and-ddd`, `syrx-data-access`, `api-design`, `critical-thinking`, etc.
3. **Curate skills** — prioritize: `syrx-data-access`, `adr-generator`, `api-design` by phase
4. **Identify instruction scope** — which canonical `.github/instructions/*.md` files are essential vs. optional/deferred

## Outputs (Save to `.docs/setup/` + `.github/agents/`)

### 1. Project Copilot Instructions
**File:** `.github/agents/project-copilot-instructions.md`

Structure:
- Overview (1–2 sentences)
- Domain Model (core aggregates, invariants)
- Canonical Standards (tied to questionnaire answers)
- Preferred Agents (with rationale)
- Preferred Skills (with rationale)
- Documentation Structure
- Getting Started

**Keep it scannable:** < 40 lines, only document what's unique to this project.

### 2. Agent Activation Rules
**File:** `.docs/setup/agents-activation.md`

Table: Agent | Activate (Yes/Optional) | Rationale

### 3. Skills Recommendations
**File:** `.docs/setup/skills-recommendations.md`

Sections: High-Priority, Medium-Priority, Deferred, Guidance by Phase

### 4. Documentation Baseline
**File:** `.docs/setup/docs-baseline.md`

Canonical structure (`.docs/plans/`, `.docs/research/`, `.docs/adr/`, `.docs/changes/`) with essential vs. optional instruction files.

### 5. Quick-Start Checklist
**File:** `.docs/setup/quick-start-checklist.md`

Actionable list of: Review instructions, confirm Syrx, scaffold repo, create ADR, write plan, next phase steps.

## Workspace Rules

- **Syrx only** — no EF Core or alternate ORMs
- **xUnit + Moq** — no FluentAssertions
- **`.docs/` canonical** — plans, research, ADRs, changes all live here
- **Guard semantics** — Syrx `Throw<TException>(condition, ...)` for validation
- **Lean standards** — only document what is unique; reference canonical instruction files for rest

## Guardrails

- Every output is tied to a questionnaire answer or workspace audit
- Every agent/skill recommendation includes rationale
- No irrelevant boilerplate — cut ruthlessly
- No vague non-goals like "Phase 2"; ask for explicit boundary
- Confirm with user before saving files
