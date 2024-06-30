namespace Zilean.ApiService.Features.Dmm;

public static class DmmConfiguration
{
    public static bool IsDmmEnabled(IConfiguration configuration)
    {
        var zileanConfig = configuration.GetSection("Zilean");
        var dmmConfig = zileanConfig.GetSection("Dmm");
        var dmmEnabled = dmmConfig.GetValue<bool>("Enabled");

        return dmmEnabled;
    }
}
