var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddConfigurationFiles();

builder.AddOtlpServiceDefaults();

builder.Services
    .AddSwaggerSupport()
    .AddSchedulingSupport()
    .AddLuceneSupport()
    .AddDmmSupport(builder.Configuration);

var app = builder.Build();

app.MapZileanEndpoints()
    .EnableSwagger();

app.Services.SetupScheduling(app.Configuration);

app.Run();
