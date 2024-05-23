using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System.Diagnostics;

var applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

var activitySourceName = "bug44163";
using ActivitySource activitySource = new ActivitySource(activitySourceName);

using IHost host = new HostBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        var openTelemetry = services.AddOpenTelemetry();
        openTelemetry.WithTracing((tracing) => tracing
            .AddSource(activitySourceName)
            .AddConsoleExporter()
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = applicationInsightsConnectionString;
            })
        );
        services.AddLogging(logging =>
        {
            logging.AddSimpleConsole();
            logging.AddOpenTelemetry(otelLogging =>
            {
                otelLogging.AddAzureMonitorLogExporter(options =>
                {
                    options.ConnectionString = applicationInsightsConnectionString;
                });
                otelLogging.AddConsoleExporter();
            });
        });
    })
    .Build();
host.Start();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("main");

var dog = "Barney";
var cat = "Whiskers";
logger.Log(LogLevel.Information, "1 Hello {dog} & {cat}!", dog, cat);
Object[] msgArgs = { dog, cat };
logger.Log(LogLevel.Information, "2 Hello {dog} & {cat}!", msgArgs);

//using Activity? activity = activitySource.CreateActivity("main", ActivityKind.Server);
//activity?.Start();
//activity?.SetTag("customTag", "customValue");
//activity?.SetTag("abc", "def");
//activity?.SetTag("http.user_agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0");
//activity?.SetTag("http.newconvetiontag", "special value");

// older tag conventions
//activity?.SetTag("http.method", "GET");
//activity?.SetTag("http.url", "http://0.0.0.0:8080/v1/skus/privateClouds");
//activity?.SetTag("http.status_code", 202); // must be an int, not a string

// newer tag conventions
//activity?.SetTag("url.full", "http://0.0.0.0:8080/v1/skus/privateClouds3");
//activity?.SetTag("http.request.method", "POST");
//activity?.SetTag("http.response.status_code", 203);
