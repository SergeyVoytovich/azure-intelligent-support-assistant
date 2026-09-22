FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY global.json ./

COPY src/Directory.Build.props ./src/
COPY src/Directory.Packages.props ./src/

COPY src/SupportAssistant.Api ./src/SupportAssistant.Api
COPY src/SupportAssistant.Application ./src/SupportAssistant.Application
COPY src/SupportAssistant.Domain ./src/SupportAssistant.Domain
COPY src/SupportAssistant.Infrastructure ./src/SupportAssistant.Infrastructure

WORKDIR /src/src

RUN dotnet restore SupportAssistant.Api/SupportAssistant.Api.csproj

RUN dotnet publish \
    SupportAssistant.Api/SupportAssistant.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS final

WORKDIR /home/site/wwwroot

COPY --from=build /app/publish .

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

EXPOSE 80