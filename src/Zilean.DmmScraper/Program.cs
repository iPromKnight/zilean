var configuration = new ConfigurationBuilder()
    .AddConfigurationFiles()
    .Build();

var zileanConfiguration = configuration.GetZileanConfiguration();

var loggerFactory = new LoggerFactory();
var loggerConfiguration = configuration.GetLoggerConfiguration();
loggerFactory.AddSerilog(loggerConfiguration.CreateLogger(), dispose: true);

return await DmmScraperTask.Execute(zileanConfiguration, loggerFactory, CancellationToken.None);
