# Security Configuration Guide

This document describes secure configuration practices for Syrx.SqlServer, covering credential management, environment setup, and CI/CD security patterns.

---

## Credential Management

### Never commit secrets

Connection strings, passwords, and API keys must **never** appear in source code or committed files.

The `.gitignore` file excludes:
- `.env` and `.env.local` files
- User secrets stores

### Environment Variables

The test infrastructure reads credentials from environment variables at runtime.

| Variable | Purpose | Required |
|----------|---------|---------|
| `MSSQL_SA_PASSWORD` | SQL Server SA password for test containers | Integration tests only |

#### Local development

```bash
# Copy the template and fill in your values
cp .env.example .env

# Edit .env — this file is git-ignored
MSSQL_SA_PASSWORD=YourStrong!Passw0rd
```

The `docker-compose.yml` in `tests/integration/Syrx.SqlServer.Tests.Integration/Docker/` reads `${MSSQL_SA_PASSWORD}` from environment, with a safe local fallback.

#### CI/CD (GitHub Actions)

Set `MSSQL_SA_PASSWORD` as an **encrypted repository secret** in GitHub → Settings → Secrets and variables → Actions.

Reference it in workflow files:

```yaml
env:
  MSSQL_SA_PASSWORD: ${{ secrets.MSSQL_SA_PASSWORD }}
```

#### Production

Never use the default `YourStrong!Passw0rd` in production. Use your secret manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) to inject the value at deploy time.

---

## Connection Strings

### Do not embed connection strings in code

Use `appsettings.json` (excluded patterns), User Secrets (development), or environment injection (production).

```csharp
// ✅ Correct: read from configuration
var connectionString = configuration.GetConnectionString("Default");

// ❌ Wrong: hardcoded
var connectionString = "Server=localhost;Password=secret;";
```

### User Secrets (development)

```bash
cd src/MyProject
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\\syrx;Database=Syrx;Integrated Security=true;"
```

User secrets are stored outside the project tree and never committed.

---

## Logging Safety

### Never log connection strings or passwords

```csharp
// ✅ Safe: log sanitized context
_logger.LogError(ex, "Database operation failed for repository {Repo}", typeof(T).Name);

// ❌ Unsafe: logs contain secrets
_logger.LogError("Failed with connection: {Conn}", connectionString);
```

The `SqlException.Message` property is safe to log — it does not include credentials. Avoid logging `SqlException.Server` or any `SqlConnectionStringBuilder` output.

---

## SQL Injection Prevention

All data access in Syrx.SqlServer is performed through:

1. **Syrx `ICommander<T>`** — always uses parameterized queries via Dapper
2. **Explicit, named SQL** defined in configuration (never constructed from user input)

This means SQL injection is structurally prevented at the framework level. No manual parameterization is required in repository code.

---

## Transport Security

- Always use `TrustServerCertificate=true` **only** in test/development environments
- In production, configure a trusted TLS certificate on the SQL Server instance and remove `TrustServerCertificate`
- Default `Microsoft.Data.SqlClient` behaviour from version 4.0+ requires explicit opt-in for unencrypted connections (`Encrypt=false`), which should never appear in production configuration

---

## Dependency Security

Dependencies are kept at current stable versions. Run the following to audit for known vulnerabilities:

```bash
dotnet list package --vulnerable
dotnet list package --outdated
```

CI includes a `dotnet list package --vulnerable` check via the GitHub Actions security workflow (`.github/workflows/security.yml`).

---

## Reporting Vulnerabilities

Please do **not** open a public GitHub issue for a security vulnerability.

Instead, use [GitHub Private Vulnerability Reporting](https://github.com/Syrx/Syrx.SqlServer/security/advisories/new) or email the maintainers directly.

---

*Last updated: 2026-03-21*
