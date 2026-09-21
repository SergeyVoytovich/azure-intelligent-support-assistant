# Unit tests

Run from the repository root:

```powershell
./src/SupportAssistant.UnitTests/Test-Coverage.ps1
```

Use `-NoRestore` once packages have been restored. The script runs all unit tests,
collects Cobertura coverage and TRX results in a unique `TestResults/verified-*`
directory, verifies that API, Application and Infrastructure are present, and fails
if tests fail or total line coverage is below 75%. It also reports branch coverage.
No production assemblies, source files or generated Functions code are excluded.

## Verified coverage

Measured on 2026-09-21 with .NET 10 and coverlet.collector:

| Metric | Result |
| --- | ---: |
| Passing tests | 151 |
| Failed / skipped tests | 0 / 0 |
| Covered lines | 665 / 794 (83.75%) |
| Covered branches | 68 / 84 (80.95%) |
| Test execution time with coverage | Approximately 1 second |

Execution time excludes package restore, compilation and test host startup.
The coverage report includes the Functions entry point and generated host code;
the tests call HTTP functions directly without starting the Functions host.

## Test design

- API tests use real request parsing and DTO mapping, checking validation,
  length boundaries, error status codes and request identifiers.
- Search and OpenAI tests use real SDK clients with an in-memory HTTP handler,
  checking outgoing request bodies and deserialization of service responses.
  Unexpected requests fail immediately. Retries are disabled in these clients.
- Ingestion tests exercise paged blob enumeration, PDF filtering, chunking,
  indexing, counters, cancellation forwarding and stream disposal on failure.
- Application tests check dependency call order, error propagation, critical
  phrases, sentiment boundaries, chunk metadata and overlap behavior.
- Dependency injection tests resolve the complete service graph and verify
  service lifetimes without obtaining credentials or contacting Azure.
- Environment-variable tests run without parallel tests and restore previous
  values in `finally` blocks.

Tests require no running services, Azure account, model deployments or API keys.
All additions and configuration changes are confined to this test project.
