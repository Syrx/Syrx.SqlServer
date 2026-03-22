# Syrx.SqlServer 3.0.0 Dependency & Package Update Analysis

**Date**: March 21, 2026  
**Target Release**: 3.0.0  
**Target Framework**: net10.0  
**Status**: Research Report (No Implementation)

---

## Executive Summary

This report documents current dependency versions across the Syrx.SqlServer repository and provides upgrade recommendations for the 3.0.0 release. Key findings include:

- **Critical Issues**: FluentAssertions (banned) present in integration tests
- **Major Updates Available**: Microsoft.Data.SqlClient 7.0.0 (breaking changes), coverlet.collector 8.0.1
- **Version Inconsistencies**: Test frameworks (xunit, xunit.runner.visualstudio) inconsistent across projects
- **Workflow Gap**: publish.yml still targets .NET 8.0.x (conflicts with net10.0 target)
- **Release Notes Outdated**: Still references .NET 8.0 and 9.0, needs .NET 10.0 update

---

## 1. NuGet Package Updates

### 1.1 Core Data Access Packages

#### Microsoft.Data.SqlClient
- **Current Version**: 6.1.4
- **Latest Stable**: 7.0.0
- **Update Type**: MAJOR VERSION (Breaking)
- **Recommendation**: EVALUATE CAREFULLY
- **Files Affected**:
  - src/Syrx.Commanders.Databases.Connectors.SqlServer/Syrx.Commanders.Databases.Connectors.SqlServer.csproj (line 12)
  - src/Syrx.SqlServer.Performance.Demo/Syrx.SqlServer.Performance.Demo.csproj (line 16)
  - tests/integration/Syrx.SqlServer.Tests.Integration.Performance/Syrx.SqlServer.Tests.Integration.Performance.csproj (line 14)

- **Breaking Changes in 7.0.0**:
  - **Azure/Entra ID Authentication Refactoring**: Azure authentication modes now require separate `Microsoft.Data.SqlClient.Extensions.Azure` package
  - **Connection String Defaults**: `Encrypt=Mandatory` is now default (stricter security)
  - **TLS 1.3 Support**: New `Encrypt=Strict` mode for TDS 8.0
  - **API Changes**: Some internal APIs have shifted for better modularity

- **Security Improvements**:
  - Enhanced encryption support with strict TLS 1.3 enforcement [✓ Positive for security standards]
  - Native support for SQL Server 2025 vector/JSON types
  - Improved Entra ID/Azure integration patterns

- **Migration Path**:
  1. Update to 7.0.0
  2. If using Entra ID authentication, add `Microsoft.Data.SqlClient.Extensions.Azure` package
  3. Review connection string formats (existing `Encrypt=Optional` should still work)
  4. Test with SQL Server 2019+ (2025+ for new features)

- **Stability**: ✓ Recommended for 3.0.0 given .NET 10.0 modernization focus

---

#### Dapper
- **Current Version**: 2.1.72
- **Latest Stable**: 2.1.72 (No newer version available)
- **Update Type**: N/A - Already at Latest
- **Recommendation**: NO ACTION
- **Files Affected**:
  - src/Syrx.Commanders.Databases.Connectors.SqlServer/Syrx.Commanders.Databases.Connectors.SqlServer.csproj (line 13)
  - src/Syrx.SqlServer.Performance.Demo/Syrx.SqlServer.Performance.Demo.csproj (line 18)
  - .submodules/Syrx.Commanders.Databases/src/Syrx.Commanders.Databases/Syrx.Commanders.Databases.csproj (line 10)

- **Status**: Latest stable release; Dapper maintains strong backwards compatibility

---

### 1.2 Microsoft.Extensions Package Suite

#### Microsoft.Extensions.DependencyInjection
- **Current Version**: 10.0.4
- **Latest Stable**: 10.0.5
- **Update Type**: PATCH
- **Recommendation**: SAFE TO UPDATE
- **Files Affected**:
  - src/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions.csproj (line 9)
  - src/Syrx.SqlServer.Performance.Demo/Syrx.SqlServer.Performance.Demo.csproj (line 17)
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions.Tests.Unit/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions.Tests.Unit.csproj (line 12)

- **Changes**: Minor security updates and dependency refinements
- **Breaking Changes**: None expected
- **Priority**: LOW - Non-blocking update

#### System.Diagnostics.PerformanceCounter
- **Current Version**: 10.0.4
- **Latest Stable**: 10.0.4 (appears latest for .NET 10)
- **Update Type**: N/A - At Latest
- **Recommendation**: NO ACTION
- **Files Affected**:
  - src/Syrx.Commanders.Databases.Connectors.SqlServer/Syrx.Commanders.Databases.Connectors.SqlServer.csproj (line 14)

#### Microsoft.Extensions.Logging & Related
- **Current Versions**: 10.0.4
- **Latest**: 10.0.5
- **Recommendation**: UPDATE (same as DependencyInjection)
- **Files Affected**:
  - src/Syrx.SqlServer.Performance.Demo/Syrx.SqlServer.Performance.Demo.csproj (lines 13-15)
  - tests/integration/Syrx.SqlServer.Tests.Integration/Syrx.SqlServer.Tests.Integration.csproj (line 16)
  - tests/integration/Syrx.SqlServer.Tests.Integration.Performance/... (not shown but referenced)
  - tests/performance/Syrx.SqlServer.Tests.Performance/Syrx.SqlServer.Tests.Performance.csproj (lines 26-27)

---

### 1.3 Build & Quality Packages

#### DotNet.ReproducibleBuilds
- **Current Version**: 1.2.39
- **Latest Stable**: 1.2.39 (appears latest)
- **Update Type**: N/A - At Latest
- **Recommendation**: NO ACTION
- **Files Affected**: All project .csproj files (Directory.Build.props baseline, Update directive in all projects)
- **Purpose**: Enables deterministic build outputs for reproducible releases

---

### 1.4 Test Framework Packages

#### xunit

**INCONSISTENCY DETECTED**: Two different versions across projects
- **Version A**: 2.9.3 (newer, used in unit and performance tests)
- **Version B**: 2.6.6 (older, used in integration.performance tests)
- **Latest Available**: 2.9.3 (v2 is now deprecated, v3 recommended for new projects)
- **Recommendation**: STANDARDIZE TO 2.9.3
- **Files Affected**:
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Tests.Unit/...csproj (line 17) - **2.9.3** ✓
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions.Tests.Unit/...csproj (line 12) - **2.9.3** ✓
  - tests/performance/Syrx.SqlServer.Tests.Performance/...csproj (line 16) - **2.9.3** ✓
  - tests/integration/Syrx.SqlServer.Tests.Integration/...csproj (line 11) - **2.9.3** ✓
  - tests/integration/Syrx.SqlServer.Tests.Integration.Performance/...csproj (line 16) - **2.6.6** ✗ OUTDATED

- **Breaking Changes**: None between 2.6.6 and 2.9.3
- **Future Note**: xUnit.net v2 is now deprecated; v3 is the future direction (plan migration for 4.0.0)

#### xunit.runner.visualstudio

**INCONSISTENCY DETECTED**: Two versions in use
- **Version A**: 3.1.5 (newer, primary)
- **Version B**: 2.8.2 (older, in integration.performance)
- **Latest Available**: 3.1.5
- **Recommendation**: STANDARDIZE TO 3.1.5
- **Files Affected**:
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Tests.Unit/...csproj (line 18) - **3.1.5** ✓
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions.Tests.Unit/...csproj (line 14) - **3.1.5** ✓
  - tests/performance/Syrx.SqlServer.Tests.Performance/...csproj (line 17) - **3.1.5** ✓
  - tests/integration/Syrx.SqlServer.Tests.Integration/...csproj (line 13) - **3.1.5** ✓
  - tests/integration/Syrx.SqlServer.Tests.Integration.Performance/...csproj (line 17) - **2.8.2** ✗ OUTDATED

- **Supports**: .NET 8.0+ and .NET Framework 4.7.2+

#### Microsoft.NET.Test.Sdk
- **Current Version**: 18.3.0
- **Latest Stable**: 18.3.0 (appears current)
- **Update Type**: N/A - At Latest
- **Recommendation**: NO ACTION
- **Files Affected**:
  - All test project .csproj files (lines 15-16 of each, commonly)

#### coverlet.collector

**INCONSISTENCY & MAJOR UPDATE AVAILABLE**: Two versions in use
- **Version A**: 6.0.4 (primary)
- **Version B**: 6.0.2 (performance tests)
- **Latest Available**: 8.0.1 (MAJOR version available)
- **Recommendation**: STANDARDIZE TO 6.0.4 for now; PLAN 8.0.1 for future
- **Files Affected**:
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Tests.Unit/...csproj (line 12) - **6.0.4** ✓
  - tests/unit/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions.Tests.Unit/...csproj (12) - **6.0.4** ✓
  - tests/performance/Syrx.SqlServer.Tests.Performance/...csproj (line 21) - **6.0.4** ✓
  - tests/integration/Syrx.SqlServer.Tests.Integration/...csproj (line 12) - **6.0.4** ✓
  - tests/integration/Syrx.SqlServer.Tests.Integration.Performance/...csproj (line 17) - **6.0.2** ✗ OUTDATED

- **Regarding 8.0.1 (MAJOR)**:
  - Requires .NET 8.0+ (Syrx.SqlServer targets net10.0, so compatible)
  - Requires SDK 8.0.414 LTS or newer (confirmed available)
  - Non-breaking change; safe for .NET 10.0 projects
  - Could be evaluated for future release if targeting coverage improvements

---

#### BenchmarkDotNet
- **Current Version**: 0.15.4
- **Latest Stable**: 0.15.8
- **Update Type**: PATCH (maintenance releases)
- **Recommendation**: SAFE TO UPDATE
- **Files Affected**:
  - tests/performance/Syrx.SqlServer.Tests.Performance/Syrx.SqlServer.Tests.Performance.csproj (line 14)

- **Changes**: Bug fixes and stability improvements; no breaking changes expected

---

### 1.5 CRITICAL ISSUE: FluentAssertions (Banned)

#### FluentAssertions
- **Current Version**: 6.12.2
- **Location**: tests/integration/Syrx.SqlServer.Tests.Integration.Performance/Syrx.SqlServer.Tests.Integration.Performance.csproj (line 13)
- **Status**: ⚠️ **VIOLATES WORKSPACE STANDARDS**
- **Workspace Rule**: "FluentAssertions is banned; use xUnit asserts"
- **File Location**: [.docs/copilot-instructions.md](../../copilot-instructions.md#code-style-and-standards) and mode instructions

- **Action Required**:
  1. Remove FluentAssertions package reference
  2. Replace all FluentAssertions assertions with xUnit assertions
  3. Update imports (remove `using FluentAssertions;`)
  4. Review assertion patterns in `tests/integration/Syrx.SqlServer.Tests.Integration.Performance/*.cs`

- **Estimated Files to Update**: 
  - tests/integration/Syrx.SqlServer.Tests.Integration.Performance/ (all .cs test files)

---

## 2. GitHub Actions Updates

### 2.1 Current Action Versions

| Action | Current | Latest | Status | File |
|--------|---------|--------|--------|------|
| `actions/checkout` | v6.0.2 | v6.0.2 | ✓ Current | publish.yml, security.yml, all workflows |
| `actions/upload-artifact` | v7.0.0 | v7.0.0 | ✓ Current | publish.yml (lines 38, 54) |
| `actions/download-artifact` | v8.0.1 | v8.0.1 | ✓ Current | publish.yml (lines 70, 104) |
| `actions/setup-dotnet` | v5.2.0 | v5.2.0 | ✓ Current | publish.yml (lines 47, 81), security.yml (43) |
| `github/codeql-action` | v4.34.1 | v4.34.1 | ✓ Current | security.yml (init, analyze) |
| `actions/dependency-review-action` | v4.9.0 | v4.9.0 | ✓ Current | security.yml (line 17) |
| `gitleaks/gitleaks-action` | v2.3.9 | v2.3.9 | ✓ Current | security.yml (65) |

**Summary**: All GitHub Actions are at latest stable versions. ✓ No updates required.

---

### 2.2 .NET SDK Versions in Workflows

#### Security Workflow (security.yml)
- **Line 43**: `dotnet-version: 10.0.x` ✓ Correct for net10.0 target
- **Status**: ✓ Already aligned with target framework

#### Publish Workflow (publish.yml)  
- **Line 51**: `dotnet-version: [ '8.0.x' ]` ⚠️ **MISMATCH**
- **Issue**: Targets .NET 8.0.x while project targets net10.0
- **Recommendation**: UPDATE TO `10.0.x`
- **Impact**: Any builds generated by publish workflow use .NET 8.0 SDK
- **Severity**: MEDIUM - Functions but not optimal for net10.0 features

---

### 2.3 Node.js Versions in Actions

All GitHub Actions in this repository use versioned action pins (preferred pattern). Node.js versions embedded in actions:

- `actions/checkout@v6.0.2`: Node.js 24 (embedded)
- `actions/upload-artifact@v7.0.0`: Node.js 24 (embedded)  
- `actions/download-artifact@v8.0.1`: Node.js 24 (embedded)
- `actions/setup-dotnet@v5.2.0`: Node.js 24 (embedded)

**Status**: ✓ All actions use modern Node.js 24 support

---

## 3. Release Notes Update Requirements

### Current Release Notes

**Location**: `Directory.Build.props`, line 25
```xml
<PackageReleaseNotes>Last release on .NET8.0 exclusively. Next release will include .NET9.0.</PackageReleaseNotes>
```

**Status**: ⚠️ **OUTDATED** - References .NET 8.0 and 9.0 as future

### Recommended Update for 3.0.0

```xml
<PackageReleaseNotes>
  - Upgraded to .NET 10.0 exclusively
  - Updated Microsoft.Data.SqlClient to 7.0.0 with enhanced Azure/Entra ID integration
  - Modernized test framework consistency (xUnit 2.9.3, xunit.runner.visualstudio 3.1.5)
  - Removed deprecated FluentAssertions; standardized on xUnit assertions
  - Updated Testcontainers.MsSql to 4.11.0
  - All GitHub Actions updated to latest stable versions
  - Reproductive build support with DotNet.ReproducibleBuilds 1.2.39
</PackageReleaseNotes>
```

### Individual Project Release Notes

Project-specific updates should also be reviewed:
- [src/Syrx.Commanders.Databases.Connectors.SqlServer/Syrx.Commanders.Databases.Connectors.SqlServer.csproj](src/Syrx.Commanders.Databases.Connectors.SqlServer/Syrx.Commanders.Databases.Connectors.SqlServer.csproj#L7-L9)
  - Current: References .NET 8.0, namespace changes
  - Should add: Microsoft.Data.SqlClient 7.0.0 migration notes

- [src/Syrx.SqlServer/Syrx.SqlServer.csproj](src/Syrx.SqlServer/Syrx.SqlServer.csproj#L5-L8)
  - Current: References .NET 8.0, extension recommendations
  - Should add: .NET 10.0 target update confirmation

---

## 4. Summary of Recommended Changes

### 🟢 SAFE - Immediate Updates (Breaking Risk: LOW)
1. **xunit**: 2.6.6 → 2.9.3 (standardize)
2. **xunit.runner.visualstudio**: 2.8.2 → 3.1.5 (standardize)
3. **coverlet.collector**: 6.0.2 → 6.0.4 (standardize)
4. **BenchmarkDotNet**: 0.15.4 → 0.15.8 (patch updates)
5. **Microsoft.Extensions.DependencyInjection**: 10.0.4 → 10.0.5 (patch)
6. **Microsoft.Extensions.Logging** suite: 10.0.4 → 10.0.5 (patch)
7. **Publish workflow**: `8.0.x` → `10.0.x` (SDK alignment)

### 🟡 REQUIRES PLANNING - Conditional Updates (Breaking Risk: MEDIUM)
1. **Microsoft.Data.SqlClient**: 6.1.4 → 7.0.0
   - Requires testing with Azure authentication paths
   - May need `Microsoft.Data.SqlClient.Extensions.Azure` package
   - Verify connection string compatibility

### 🔴 MUST FIX - Violations (Breaking Risk: HIGH)
1. **Remove FluentAssertions** 6.12.2
   - Violates workspace standards
   - Replace with xUnit assertions

### ⚪ NO ACTION - Already Current
- Dapper (2.1.72)
- System.Diagnostics.PerformanceCounter (10.0.4)
- DotNet.ReproducibleBuilds (1.2.39)
- Microsoft.NET.Test.Sdk (18.3.0)
- All GitHub Actions (latest versions)

---

## 5. Version Consistency Matrix

| Package | Current | Recommended | Standard | Files Affected |
|---------|---------|-------------|----------|-----------------|
| xunit | 2.6.6 / 2.9.3 | 2.9.3 | ✓ Yes | 5 .csproj files |
| xunit.runner.visualstudio | 2.8.2 / 3.1.5 | 3.1.5 | ✓ Yes | 5 .csproj files |
| coverlet.collector | 6.0.2 / 6.0.4 | 6.0.4 | ✓ Yes | 5 .csproj files |

---

## 6. Risk Assessment

### High Priority
- **FluentAssertions removal** (Standards violation, MUST implement)
- **Microsoft.Data.SqlClient 7.0.0** (Breaking API changes, requires validation)

### Medium Priority  
- **Test framework standardization** (2 versions each of xunit/xunit.runner.visualstudio)
- **Publish workflow .NET version** (Targets wrong SDK)

### Low Priority
- **Patch updates** (DependencyInjection, BenchmarkDotNet, etc.)
- **Future consideration**: coverlet.collector 8.0.1, xUnit v3 migration

---

## 7. Implementation Order (When Ready)

1. **Phase 1 - Critical Fixes** (MUST for 3.0.0):
   - Remove FluentAssertions, replace with xUnit assertions
   - Update Directory.Build.props release notes

2. **Phase 2 - Framework Standardization**:
   - Standardize xunit to 2.9.3 across all projects
   - Standardize xunit.runner.visualstudio to 3.1.5 across all projects
   - Standardize coverlet.collector to 6.0.4 across all projects

3. **Phase 3 - SDK & Build Updates**:
   - Update publish.yml workflow: `8.0.x` → `10.0.x`
   - Update patch versions (DependencyInjection, Extensions.Logging)
   - Update BenchmarkDotNet to 0.15.8

4. **Phase 4 - Major Dependency (Separate PR)**:
   - Evaluate Microsoft.Data.SqlClient 7.0.0 migration
   - Create migration guide for connection string changes
   - Test Azure/Entra ID authentication paths
   - Consider Testcontainers.MsSql 4.11.0 upgrade

---

## 8. References

- [Microsoft.Data.SqlClient 7.0.0 Release](https://www.nuget.org/packages/Microsoft.Data.SqlClient/7.0.0)
- [xUnit.net 2.9.3 Release](https://www.nuget.org/packages/xunit/2.9.3)
- [Testcontainers 4.11.0 Release](https://www.nuget.org/packages/Testcontainers.MsSql/4.11.0)
- [GitHub Actions Latest Releases](https://github.com/actions)
- [Workspace Standards - FluentAssertions Ban](../../copilot-instructions.md)

---

**Report Prepared**: March 21, 2026 | **Status**: Research Only (No Changes Implemented)
