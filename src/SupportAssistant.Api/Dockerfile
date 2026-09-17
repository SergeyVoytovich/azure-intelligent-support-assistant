FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore SupportAssistant.slnx

RUN dotnet publish SupportAssistant.Api/SupportAssistant.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final

WORKDIR /home/site/wwwroot

COPY --from=build /app/publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

EXPOSE 80
