var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigurationFiles();

var zileanConfiguration = builder.Configuration.GetZileanConfiguration();

builder.AddOtlpServiceDefaults();

builder.Services
    .AddConfiguration(zileanConfiguration)
    .AddSwaggerSupport()
    .AddSchedulingSupport()
    .AddElasticSearchSupport()
    .AddDmmSupport(zileanConfiguration)
    .AddConditionallyRegisteredHostedServices(zileanConfiguration);

var app = builder.Build();

app.MapZileanEndpoints(zileanConfiguration)
    .EnableSwagger();

app.Services.SetupScheduling(zileanConfiguration);

app.Run();
