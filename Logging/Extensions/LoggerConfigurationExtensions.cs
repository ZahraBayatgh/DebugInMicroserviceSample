using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace Hasti.Framework.Endpoints.Logging.Extensions;
public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration WithDefaultConfiguration(this LoggerConfiguration loggerConfig, HostBuilderContext hostBuilderContext,IServiceProvider serviceProvider)
    {
        IConfiguration configuration = hostBuilderContext.Configuration;
        string? elasticsearchUri = configuration["ConnectionStrings:ElasticConnectionString"];

        string? assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

        IHostEnvironment hostEnvironment = hostBuilderContext.HostingEnvironment;
        loggerConfig
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", hostEnvironment.ApplicationName)
            .Enrich.WithProperty("EnvironmentName", hostEnvironment.EnvironmentName)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .MinimumLevel.Override("DotNetCore.CAP", LogEventLevel.Error)
            .MinimumLevel.Override("WebApi", LogEventLevel.Debug)
             .Enrich.With<ActivityEnricher>()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Assembly", assemblyName);

        loggerConfig.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
            .WithDefaultDestructurers()
            .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }));

        loggerConfig.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");

        if (!string.IsNullOrWhiteSpace(elasticsearchUri))
        {
            loggerConfig.WriteTo.Logger(lc =>
                        lc.Filter.ByExcluding(Matching.WithProperty<bool>("Security", p => p))
                            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                            {
                                AutoRegisterTemplate = true,
                                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                IndexFormat = $"{assemblyName!.ToLower().Replace(".", "-")}-{hostEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                                //IndexFormat = "logs-{0:yyyy.MM.dd}",
                                BatchAction = ElasticOpType.Create,
                                TypeName = null,
                                InlineFields = true,
                                FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback
                            }))
                    .WriteTo.Logger(lc =>
                        lc.Filter.ByIncludingOnly(Matching.WithProperty<bool>("Security", p => p))
                            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                            {
                                AutoRegisterTemplate = true,
                                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                IndexFormat = "security-{0:yyyy.MM.dd}",
                                BatchAction = ElasticOpType.Create,
                                InlineFields = true
                            }));
        }

        loggerConfig.ReadFrom.Configuration(configuration); // minimum levels defined per project in json files 

        return loggerConfig;
    }
}

