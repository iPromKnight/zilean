var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigurationFiles();

var zileanConfiguration = builder.Configuration.GetZileanConfiguration();

builder.AddOtlpServiceDefaults();

builder.Services
    .AddSwaggerSupport()
    .AddSchedulingSupport()
    .AddLuceneSupport()
    .AddDmmSupport(zileanConfiguration);

var app = builder.Build();

app.MapZileanEndpoints(zileanConfiguration)
    .EnableSwagger();

app.Services.SetupScheduling(zileanConfiguration);

app.Run();
