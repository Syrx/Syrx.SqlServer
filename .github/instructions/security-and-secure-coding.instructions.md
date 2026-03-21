---
name: security-and-secure-coding
description: 'Consolidated secure coding rules for .NET-focused work, aligned with OWASP and workspace conventions.'
applyTo: '**'
---
# Security & Secure Coding

## Core Principles
- Least privilege.
- Deny by default.
- Explicit allow-lists for external systems, URLs, and privileged actions.

## Input Validation
- Validate all external input at the boundary.
- Use Syrx guard semantics for invalid input and fail fast before state mutation.
- Reject path traversal, malformed identifiers, and invalid enum or range values immediately.

## Secrets & Sensitive Data
- Never hardcode secrets, connection strings, or tokens.
- Load secrets from environment variables or an approved secret manager.
- Redact secrets and sensitive identifiers from logs.

## Data Access & Injection
- Use explicit, parameterized SQL only.
- Do not concatenate SQL, shell commands, URLs, or file paths from untrusted input.
- For .NET data access, use Syrx only.

## Web & API Security
- Validate outbound user-supplied URLs to prevent SSRF.
- Apply authentication and authorization checks explicitly at protected boundaries.
- Prefer secure defaults for cookies, transport, and API surface configuration.

## Cryptography
- Use modern, approved algorithms.
- Prefer HTTPS for transport.
- Use strong hashing for passwords and standard encryption for sensitive data at rest when required.

## Error Handling & Observability
- No silent catches.
- Log with actionable context, then rethrow or translate to a safe exception.
- Do not expose raw internal errors, SQL details, or secrets to callers.

## Dependency Hygiene
- Prefer current stable package versions.
- Run security scanning as part of CI for dependencies and code.

