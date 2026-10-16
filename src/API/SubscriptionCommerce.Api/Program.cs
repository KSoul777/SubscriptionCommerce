using System.Reflection;
using System.Text.Json.Serialization;
using Serilog;
using SubscriptionCommerce.Api.Extensions;
using SubscriptionCommerce.Api.Middleware;
using SubscriptionCommerce.Api.OpenTelemetry;
using SubscriptionCommerce.Common.Application;
using SubscriptionCommerce.Common.Infrastructure;
using SubscriptionCommerce.Common.Presentation.Endpoints;
using SubscriptionCommerce.Modules.Customers.Infrastructure;
using SubscriptionCommerce.Modules.Pricing.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

Assembly[] moduleApplicationAssemblies =
[
    SubscriptionCommerce.Modules.Pricing.Application.AssemblyReference.Assembly,
    SubscriptionCommerce.Modules.Customers.Application.AssemblyReference.Assembly
];

builder.Services.AddApplication(moduleApplicationAssemblies);

builder.Services.AddInfrastructure(DiagnosticsConfig.ServiceName);

builder.Services.AddPricingModule(builder.Configuration);

builder.Services.AddCustomersModule(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseLogContextTraceLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.MapEndpoints();

await app.RunAsync();

public partial class Program;
