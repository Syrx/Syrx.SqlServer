# PRD: {Project Title}

## 1. Product overview

### 1.1 Document title and version
- PRD: {Project Title}
- Version: {version}
- Status: Draft | Review | Approved
- Owner: {name/role}
- Last updated: {date}

### 1.2 Problem statement
Two or three sentences: what user pain or business gap does this address, and why now?

### 1.3 Product summary
Two or three short paragraphs covering context, strategic fit, and high-level solution approach.

## 2. Goals

### 2.1 Business goals
- {REQ-B01} Must {measurable business outcome}.

### 2.2 User goals
- {REQ-U01} Must allow {persona} to {accomplish task} so that {benefit}.

### 2.3 Non-goals (explicitly out of scope)
- {item}: will not be addressed in this release because {reason}.

## 3. User personas

### 3.1 Key user types
- {Persona name}: {one-line description}

### 3.2 Persona details
**{Persona name}**
- Role: {role}
- Goal: {primary goal}
- Pain point: {key friction this feature resolves}

### 3.3 Role-based access
| Role | Permissions |
|---|---|
| {role} | {can/cannot actions} |

## 4. Security requirements

> Security requirements are captured here before functional requirements to ensure they are never retrofitted.

- {SEC-001} (Must) {authentication/authorisation requirement}
- {SEC-002} (Must) {data privacy requirement — PII handling, encryption at rest/transit}
- {SEC-003} (Must) {input validation and injection prevention requirement}
- {SEC-004} (Should) {audit logging requirement}

## 5. Functional requirements

| ID | Priority | Requirement | Linked persona | Linked AC |
|---|---|---|---|---|
| REQ-001 | Must | {requirement statement using "must/will"} | {persona} | AC-001 |

## 6. Interface & data contracts

### 6.1 API endpoints / events
- Method, path/topic, request shape, response shape, error codes.

### 6.2 Data models
- Key entities, field names, types, constraints.

### 6.3 Integration points
- External systems, third-party APIs, internal services this feature depends on.

## 7. User experience

### 7.1 Entry points & first-time user flow
- How does a user first encounter this feature?

### 7.2 Core experience
**{Step name}**: {description of what the user does and sees}
- How this step prevents errors or ensures a positive outcome.

### 7.3 Edge cases & error states
- {scenario}: expected system behaviour and user-facing message.

### 7.4 UI/UX highlights
- Key design constraints, accessibility requirements (WCAG level), responsive breakpoints.

## 8. Narrative

One paragraph: describe the end-to-end user journey in plain language, illustrating the value delivered.

## 9. Success metrics

| Metric | Baseline | Target | Measurement method |
|---|---|---|---|
| {user metric} | {current} | {goal} | {how measured} |
| {business metric} | {current} | {goal} | {how measured} |
| {technical metric} | {current} | {goal} | {how measured} |

## 10. Risks & dependencies

### 10.1 Risk register
| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| {risk description} | High/Med/Low | High/Med/Low | {mitigation action} |

### 10.2 Dependencies
- {internal/external dependency}: required by {phase/date}, owner {team}.

### 10.3 Assumptions
- {assumption}: if invalidated, {impact on scope}.

## 11. Milestones & sequencing

### 11.1 Project estimate
- Size: {XS/S/M/L/XL} — {time estimate}

### 11.2 Team size & composition
- {n} engineers ({roles}), {n} designers, {n} QA

### 11.3 Suggested phases
**Phase 1 — {name}** ({estimate})
- Deliverables: {list}

**Phase 2 — {name}** ({estimate})
- Deliverables: {list}

## 12. User stories

### 12.{x}. {User story title}
- **ID**: GH-{###}
- **As a** {persona}, **I want to** {action} **so that** {benefit}.
- **Priority**: Must | Should | Could | Won't
- **Linked requirements**: REQ-{###}, SEC-{###}
- **Acceptance criteria**:
  - AC-{###}: {binary, testable condition — given/when/then format preferred}
  - AC-{###}: {binary, testable condition}
