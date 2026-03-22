---
name: testing-strategy
description: 'Unified testing strategy: naming, patterns, and tooling.'
applyTo: '**/*.cs'
---
# Testing Strategy

## Naming & Structure
`{Scenario}{ExpectedBehaviour}` for all tests. AAA pattern; one assertion concept per test.

## Tools
xUnit + Moq only. FluentAssertions banned.

## Coverage Focus
Prioritize domain invariants, repository behavior (with mocks), and error paths.

## Guards
Test invalid inputs trigger expected exceptions using Syrx guard semantics.

