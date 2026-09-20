using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAssistant.Api.Configuration;
using SupportAssistant.Api.Mapping;
using SupportAssistant.Api.System;
using SupportAssistant.Infrastructure.DependencyInjection;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (EnvironmentVariables.IsApplicationinsightsConnected)
{
    builder.Services
        .AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}


builder.Services.AddInfrastructure(builder.Configuration.GetInfrastructureConfiguration());

builder.Services.AddAutoMapper(cnf => cnf.AddProfile<DtoProfile>());

builder.Build().Run();
