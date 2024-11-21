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
    .AddStartupHostedServices()
    .AddDashboardSupport(zileanConfiguration);

var app = builder.Build();

app.UseZileanRequired(zileanConfiguration);
app.MapZileanEndpoints(zileanConfiguration);
app.Services.SetupScheduling(zileanConfiguration);

app.Run();
