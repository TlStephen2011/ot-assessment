using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OT.Assessment.App.Contracts;
using OT.Assessment.App.Implementations;
using OT.Assessment.Domain.Configuration.Options;
using OT.Assessment.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

var seqApiKey = builder.Configuration.GetValue<string>("Seq:ApiKey");

if (!string.IsNullOrWhiteSpace(seqApiKey))
{
    builder.Logging.AddOpenTelemetry(x =>
    {
        x.SetResourceBuilder(ResourceBuilder.CreateEmpty()
            .AddService(builder.Configuration.GetValue<string>("ServiceName"))
            .AddAttributes(new Dictionary<string, object>()
            {
                ["deployment.environment"] = builder.Environment.EnvironmentName
            }));

        x.IncludeScopes = true;
        x.IncludeFormattedMessage = true;

        x.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri($"{builder.Configuration.GetValue<string>("Seq:Url")}/ingest/otlp/v1/logs");
            o.Protocol = OtlpExportProtocol.HttpProtobuf;
            o.Headers = $"X-Seq-ApiKey={seqApiKey}";
        });
    });
}

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddControllers();

builder.Services.AddRouting(o => o.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICasinoWagerPublishService, CasinoWagerPublishService>();
builder.Services.AddScoped<ICasinoService, CasinoService>();
builder.Services.AddScoped<ICasinoWagerRepository, CasinoWagerRepository>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opts =>
{
    opts.EnableTryItOutByDefault();
    opts.DocumentTitle = "OT Assessment App";
    opts.DisplayRequestDuration();
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
