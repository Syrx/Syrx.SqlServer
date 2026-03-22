---
name: api-design
description: >
  **SKILL** - Design resilient API clients and service integrations with clear layering and policy-based resilience.
  USE FOR: external HTTP integrations, DTO design, retries, backoff, circuit breakers, and API client abstractions.
  DO NOT USE FOR: general .NET coding tasks unrelated to integration boundaries.
---

# API Design Skill

## Mandatory Inputs

- Language
- Endpoint or API surface
- At least one HTTP verb or operation

Optional inputs:

- Request and response DTOs
- Authentication model
- Resilience requirements such as retry, timeout, circuit breaker, bulkhead, or throttling
- Required tests

## Design Model

Use a three-layer pattern when it adds value:

1. Service: raw transport and protocol handling
2. Manager: orchestration and abstraction
3. Resilience: policy execution and failure handling

## .NET-Specific Rules

- Use `HttpClient` correctly through DI.
- Prefer records for immutable DTOs.
- Validate all public entry points.
- Map errors explicitly instead of leaking transport exceptions blindly.
- Use Polly or the team's current resilience stack when resilience is required.

## Security & Reliability

- Never hardcode secrets.
- Prefer HTTPS.
- Validate outbound URLs and configuration.
- Define timeout, retry, and failure behavior deliberately.