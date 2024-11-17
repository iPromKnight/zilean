var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigurationFiles();

var zileanConfiguration = builder.Configuration.GetZileanConfiguration();

builder.AddOtlpServiceDefaults();

builder.Services
    .AddConfiguration(zileanConfiguration)
    .AddSwaggerSupport()
    .AddSchedulingSupport()
    .AddShellExecutionService()
    .ConditionallyRegisterDmmJob(zileanConfiguration)
    .AddZileanDataServices(zileanConfiguration)
    .AddApiKeyAuthentication()
    .AddStartupHostedServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapDefaultEndpoints();
app.MapZileanEndpoints(zileanConfiguration)
    .EnableSwagger();

app.Services.SetupScheduling(zileanConfiguration);

logger.LogInformation("Zilean API Service started.");

app.Run();
