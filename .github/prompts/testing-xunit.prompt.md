---
name: testing-xunit
description: 'Prompt for XUnit test generation aligned with project standards and workspace testing rules.'
---
# XUnit Testing Prompt

Generate tests named `{Scenario}{ExpectedBehavior}` using xUnit and Moq, aligned with workspace rules.

## Project Setup

- Use a separate test project named `[ProjectName].[TestProjectType].Tests`.
- Use `Microsoft.NET.Test.Sdk`, `xunit`, and `xunit.runner.visualstudio`.
- Keep test folders and organization aligned with the production code under test.

## Test Structure

- Use `[Fact]` for simple tests and `[Theory]` for data-driven tests.
- Follow the AAA pattern without adding Arrange/Act/Assert comments.
- Keep tests focused on one behavior.
- Make tests independent, repeatable, and safe to run in any order.
- Use constructor setup, `IDisposable`, `IClassFixture<T>`, or `ICollectionFixture<T>` where appropriate.

## Assertions and Exceptions

- Assert with xUnit only.
- Use `Assert.Equal`, `Assert.True`, `Assert.False`, `Assert.Contains`, and other xUnit assertions as appropriate.
- Use `Assert.Throws<T>` and `Assert.ThrowsAsync<T>` for exception behavior.
- FluentAssertions is banned.

## Mocking

- Use Moq for external dependencies only.
- Prefer testing through public APIs.
- Avoid mocking code that is part of the solution under test.

## Coverage Focus

- Test changed behavior and edge cases.
- Ensure guard failures are covered.
- Prefer tests that validate business behavior over internal implementation detail.

