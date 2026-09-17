using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAssistant.Api.Mapping;
using SupportAssistant.Application.Chats;
using SupportAssistant.Infrastructure.Chats;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAnswerGenerator, StubAnswerGenerator>();
builder.Services.AddAutoMapper(cnf => cnf.AddProfile<DtoProfile>());

builder.Build().Run();
