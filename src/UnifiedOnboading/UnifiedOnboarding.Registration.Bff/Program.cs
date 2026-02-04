using Serilog;
using UnifiedOnboarding.Registration.Bff.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Configuration
    .AddJsonFile("ConfigFiles/Serilog.config.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ConfigFiles/RateLimit.config.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ConfigFiles/Auth.config.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ConfigFiles/Polly.config.json", optional: false, reloadOnChange: true);


ConfigurationManager config = builder.Configuration;

builder.Host.UseSerilog((context, services, LoggerConfiguration) =>
    LoggerConfiguration.ReadFrom.Configuration(builder.Configuration)
);

// 1. Infrastructure (logging, rate limiting, mediator, resiliency)
builder.Services.AddInfrastructureServices(config);

// 2. Vertical Slice feature modules
builder.Services.AddApiFeatures();

WebApplication app = builder.Build();
app.MapDefaultEndpoints();

// 3. Middlewares
app.UseBffMiddlewares();

// 4. Endpoint mappings
app.MapGet("/", () => "UP and Running!")
    .RequireRateLimiting("Anonymous");
app.MapApiFeatures();

app.Run();
