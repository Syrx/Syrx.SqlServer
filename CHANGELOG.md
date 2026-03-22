# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [3.0.0] - 2026-Q2

### Added
- OpenTelemetry instrumentation support (see `.docs/PERFORMANCE.md`)
- Secure configuration documentation (`.docs/SECURITY.md`)
- Production baseline performance metrics

### Changed
- **BREAKING**: .NET 10.0 is now the minimum supported version
- Upgraded xUnit test frameworks (2.6.6 -> 2.9.3, runner 2.8.2 -> 3.1.5)
- Modernized dependency versions
- Externalized credential management

### Fixed
- SEC-001 through SEC-007: Security hardening
  - Removed hardcoded passwords from test fixtures
  - Externalized database credentials to environment variables
  - Improved error message sanitization to prevent credential exposure

### Deprecated
- Support for .NET 8.0 and 9.0 (use 2.x releases for earlier versions)

### Security
- See `.docs/SECURITY.md` for credential handling and secure configuration patterns
- All dependencies at current stable versions

## [2.2.0] - [Previous Release Date]
...existing changelog entries...
