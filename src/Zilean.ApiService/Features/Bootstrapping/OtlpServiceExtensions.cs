using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;
using SimCube.Aspire.Features.Seq;

namespace Zilean.ApiService.Features.Bootstrapping;

public static class OtlpServiceExtensions
{
    private const string ConsoleOutputFormat = "[{Timestamp:HH:mm:ss}] | {Level:u4} | {SourceContext} | {Message:lj}{NewLine}{Exception}";

    public static void AddOtlpServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureSerilog();

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });
    }

    private static void ConfigureSerilog(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSerilog(config =>
        {
            config.ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithSpan()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers([ new DbUpdateExceptionDestructurer() ]))
                .Enrich.WithProperty(nameof(builder.Environment), builder.Environment.EnvironmentName)
                .Enrich.WithProperty(nameof(OtlpLiterals.ServiceName), builder.Configuration[OtlpLiterals.ServiceName])
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: ConsoleOutputFormat);

            if (!string.IsNullOrEmpty(builder.Configuration[OtlpLiterals.Endpoint]))
            {
                config.WriteTo.OpenTelemetry(options =>
                {
                    options.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                    options.Endpoint = builder.Configuration[OtlpLiterals.Endpoint];
                    AddHeaders(options.Headers, builder.Configuration[OtlpLiterals.Headers]);
                    AddResourceAttributes(options.ResourceAttributes, builder.Configuration[OtlpLiterals.ResourceAttributes]);
                    options.ResourceAttributes.Add("service.name", builder.Configuration[OtlpLiterals.ServiceName]);
                });
            }

            if (!string.IsNullOrEmpty(builder.Configuration[SeqLiterals.SeqEndpoint]))
            {
                config.WriteTo.Seq(builder.Configuration[SeqLiterals.SeqEndpoint]);
            }
        });
    }

    private static void ConfigureOpenTelemetryLogging(this IHostApplicationBuilder builder) =>
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

    private static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        if (string.IsNullOrEmpty(builder.Configuration[OtlpLiterals.Endpoint]))
        {
            return;
        }

        builder.ConfigureOpenTelemetryLogging();

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation(options => options.FilterHttpRequestMessage = request =>
                        !request.RequestUri?.AbsoluteUri.Contains(builder.Configuration[OtlpLiterals.Endpoint],
                            StringComparison.Ordinal) ?? true);
            });

        builder.AddOpenTelemetryExporters();
    }

    private static void AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration[OtlpLiterals.Endpoint]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }
    }

    private static void AddDefaultHealthChecks(this IHostApplicationBuilder builder) =>
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

    private static void AddResourceAttributes(IDictionary<string, object> attributes, string attributeConfig)
    {
        if (!string.IsNullOrEmpty(attributeConfig))
        {
            var parts = attributeConfig.Split('=');

            if (parts.Length == 2)
            {
                attributes[parts[0]] = parts[1];
                return;
            }

            throw new InvalidOperationException($"Invalid resource attribute format: {attributeConfig}");
        }
    }

    private static void AddHeaders(IDictionary<string, string> headers, string headerConfig)
    {
        if (!string.IsNullOrEmpty(headerConfig))
        {
            foreach (var header in headerConfig.Split(','))
            {
                var parts = header.Split('=');

                if (parts.Length == 2)
                {
                    headers[parts[0]] = parts[1];
                    continue;
                }

                throw new InvalidOperationException($"Invalid header format: {header}");
            }
        }
    }
}
