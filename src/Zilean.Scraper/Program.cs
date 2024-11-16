var builder = Host.CreateDefaultBuilder();

builder.ConfigureAppConfiguration(configuration =>
{
    configuration.AddConfigurationFiles();
});

builder.ConfigureLogging((context, logging) =>
{
    logging.ClearProviders();
    var loggingConfiguration = context.Configuration.GetLoggerConfiguration();
    Log.Logger = loggingConfiguration.CreateLogger();
    logging.AddSerilog();
});

builder.ConfigureServices((context, services) =>
{
    services.AddScrapers(context.Configuration);
    services.AddCommandLine<DefaultCommand>(config =>
    {
        config.SetApplicationName("zilean-scraper");

        config.AddCommand<DmmSyncCommand>("dmm-sync")
            .WithDescription("Sync DMM Hashlists from Github.");

        config.AddCommand<GenericSyncCommand>("generic-sync")
            .WithDescription("Sync data from Zurg and Zilean instances.");
    });
});

var host = builder.Build();
return await host.RunAsync(args);
