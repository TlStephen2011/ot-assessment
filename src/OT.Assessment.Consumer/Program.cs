using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OT.Assessment.Consumer;
using OT.Assessment.Domain.Configuration.Options;
using Serilog;
using System.Configuration;
using OT.Assessment.Infrastructure.Repositories;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqOptions>(context.Configuration.GetSection("RabbitMQ"));
        services.AddAkkaClientServices(context.Configuration);
        services.AddScoped<ICasinoWagerRepository, CasinoWagerRepository>();

        string seqApiKey = context.Configuration["Seq:ApiKey"];
        if (!string.IsNullOrWhiteSpace(seqApiKey))
        {
            services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateEmpty()
                        .AddService(context.Configuration.GetValue<string>("ServiceName"))
                        .AddAttributes(new Dictionary<string, object>
                        {
                            ["deployment.environment"] = context.HostingEnvironment.EnvironmentName
                        }));

                    options.IncludeScopes = true;
                    options.IncludeFormattedMessage = true;

                    options.AddOtlpExporter(o =>
                    {
                        o.Endpoint =
                            new Uri($"{context.Configuration.GetValue<string>("Seq:Url")}/ingest/otlp/v1/logs");
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                        o.Headers = $"X-Seq-ApiKey={seqApiKey}";
                    });
                });
            });
        }
        else
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });
        }
    })
    .Build();


var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

await host.RunAsync();

logger.LogInformation("Application ended {time:yyyy-MM-dd HH:mm:ss}", DateTime.Now);