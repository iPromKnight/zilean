var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddSwaggerSupport()
    .AddDmmSupport();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapZileanEndpoints()
    .EnableSwagger();

app.Services.ScheduleDmmJobs();

app.Run();
