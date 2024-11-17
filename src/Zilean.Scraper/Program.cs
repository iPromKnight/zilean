﻿var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddConfigurationFiles();

builder.AddOtlpServiceDefaults();

builder.Services.AddDmmScraper(builder.Configuration);

var scraper = builder.Build();

await scraper.RunAsync();
