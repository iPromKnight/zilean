namespace Zilean.ApiService.Features.Bootstrapping;

public static class WebApplicationExtensions
{
    public static WebApplication UseZileanRequired(this WebApplication app, ZileanConfiguration configuration)
    {
        if (configuration.EnableDashboard)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseAntiforgery();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapZileanEndpoints(this WebApplication app, ZileanConfiguration configuration)
    {
        app.MapDefaultEndpoints();

        app.MapDmmEndpoints(configuration)
            .MapImdbEndpoints(configuration)
            .MapTorznabEndpoints(configuration)
            .MapTorrentsEndpoints(configuration)
            .MapBlacklistEndpoints()
            .MapHealthCheckEndpoints();

        if (configuration.EnableDashboard)
        {
            app.MapStaticAssets();

            app.MapRazorComponents<Dashboard.Components.ZileanWebApp>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode();

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzU4OTgxNkAzMjM3MmUzMDJlMzBuY2dtNzRCZjAzRmtPTDdGcmFRNXVXTDhTOHdjaU9sNDZPUjBWMEsxSmlNPQ==");
        }

        app.MapOpenApi();
        app.MapScalarApiReference();

        return app;
    }
}
