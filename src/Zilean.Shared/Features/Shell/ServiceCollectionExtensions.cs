namespace Zilean.Shared.Features.Shell;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShellExecutionService(this IServiceCollection services)
    {
        services.AddSingleton<IShellExecutionService, ShellExecutionService>();
        return services;
    }
}
