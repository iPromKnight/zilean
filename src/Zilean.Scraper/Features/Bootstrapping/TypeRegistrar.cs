namespace Zilean.Scraper.Features.Bootstrapping;

internal sealed class TypeRegistrar(IServiceCollection provider) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(provider.BuildServiceProvider());

    public void Register(Type service, Type implementation) => provider.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => provider.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory) => provider.AddSingleton(service, _ => factory());
}
