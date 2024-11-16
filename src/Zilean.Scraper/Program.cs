var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddConfigurationFiles();

builder.AddOtlpServiceDefaults();

builder.Services.AddScrapers(builder.Configuration);

var scraper = builder.Build();

await scraper.RunAsync();
