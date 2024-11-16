namespace Zilean.Scraper.Features.Bootstrapping;

public static class HostingExtensions
{
    public static IServiceCollection AddCommandLine(
        this IServiceCollection services,
        Action<IConfigurator> configurator)
    {
        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(configurator);
        services.AddSingleton<ICommandApp>(app);

        return services;
    }

    public static IServiceCollection AddCommandLine<TDefaultCommand>(
        this IServiceCollection services,
        Action<IConfigurator> configurator)
        where TDefaultCommand : class, ICommand
    {
        var app = new CommandApp<TDefaultCommand>(new TypeRegistrar(services));
        app.Configure(configurator);
        services.AddSingleton<ICommandApp>(app);

        return services;
    }

    public static async Task<int> RunAsync(this IHost host, string[] args)
    {
        ArgumentNullException.ThrowIfNull(host);

        await host.StartAsync();

        try
        {
            var app = host.Services.GetService<ICommandApp>() ??
                      throw new InvalidOperationException("Command application has not been configured.");

            return await app.RunAsync(args);
        }
        finally
        {
            await host.StopAsync();
            await ((IAsyncDisposable)host).DisposeAsync();
        }
    }
}
