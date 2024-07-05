namespace Zilean.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddConditionallyRegisteredHostedServices(this IServiceCollection services, ZileanConfiguration configuration)
    {
        var hostedServiceType = typeof(IConditionallyRegisteredHostedService);
        var hostedServices = typeof(Program).Assembly.GetTypes()
            .Where(type => hostedServiceType.IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false });

        foreach (var hostedService in hostedServices)
        {
            var method = hostedService.GetMethod("ConditionallyRegister", BindingFlags.Static | BindingFlags.Public) ??
                         throw new InvalidOperationException(
                             $"The class {hostedService.FullName} must implement a static method 'ConditionallyRegister'.");

            method.Invoke(null, [services, configuration]);
        }
    }
}
