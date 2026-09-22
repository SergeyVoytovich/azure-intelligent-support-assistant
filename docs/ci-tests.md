# CI and integration tests

`pipelines/pr-ci.yml` builds the whole solution and runs .NET unit tests,
architecture tests and frontend tests. The build keeps `--no-restore` and
`-warnaserror`. A direct restore of the Function App prepares its generated
extension project before build (AZFW0108).

`pipelines/integration-tests.yml` is a separate, manual Azure DevOps pipeline.
It has no push or PR trigger. Run it after the dev Function App has been deployed.
It runs all six existing integration tests against the deployed API and Azure
services, using the existing `sc-isa-dev-wif` service connection and Azure CLI
authentication. It does not deploy or modify Azure resources.

The service connection must be authorized for this pipeline and have access to
read the Function App and use the existing Search, Language and OpenAI services.
The Search test requires seeded warranty documents in `knowledge-index`.
Missing access, unavailable services and failed assertions fail the run; the
tests are not silently skipped.

For local integration testing, sign in with `az login` and start the API on
`http://localhost:7071`, or set `SUPPORT_ASSISTANT_API_BASE_URL` to a deployed API
base URL. Then run:

```powershell
dotnet restore src/SupportAssistant.Api/SupportAssistant.Api.csproj
dotnet test src/SupportAssistant.IntegrationTests/SupportAssistant.IntegrationTests.csproj
```
