var configuration = new ConfigurationBuilder()
    .AddConfigurationFiles()
    .Build();

var zileanConfiguration = configuration.GetZileanConfiguration();

var loggerFactory = new LoggerFactory();
var loggerConfiguration = configuration.GetLoggerConfiguration();
loggerFactory.AddSerilog(loggerConfiguration.CreateLogger(), dispose: true);

var result = await DmmScraperTask.Execute(zileanConfiguration, loggerFactory, CancellationToken.None);
Environment.ExitCode = result;
Process.GetCurrentProcess().Kill();
