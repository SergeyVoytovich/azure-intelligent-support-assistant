FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS base
WORKDIR /src

COPY . .

RUN dotnet restore SupportAssistent.slnx

RUN dotnet publish SupportAssistant.Api/SupportAssistant.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final

WORKDIR /home/site/wwwroot

ENV AzureWebJobsScriptRoot=/home/site/wwwroot FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

EXPOSE 80
