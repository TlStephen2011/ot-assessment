using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OT.Assessment.App.Contracts;
using OT.Assessment.Domain.Configuration.Options;
using OT.Assessment.Domain.Models;
using RabbitMQ.Client;

namespace OT.Assessment.App.Implementations;

public class CasinoWagerPublishService : ICasinoWagerPublishService
{
    private readonly ILogger<CasinoWagerPublishService> _logger;
    private readonly RabbitMqOptions _rabbitSettings;
    private IConnection _connection;
    private const string Exchange = "AssessmentExchange";
    private IChannel _channel;
    private readonly IConnectionFactory _factory;

    public CasinoWagerPublishService(IOptions<RabbitMqOptions> rabbitOptions, ILogger<CasinoWagerPublishService> logger)
    {
        _logger = logger;
        _rabbitSettings = rabbitOptions.Value;

        _factory = new ConnectionFactory()
        {
            HostName = _rabbitSettings.Host,
            Port = _rabbitSettings.Port,
            UserName = _rabbitSettings.Username,
            Password = _rabbitSettings.Password
        };

    }

    private async Task EnsureConnection()
    {
        _connection ??= await _factory.CreateConnectionAsync();
        _channel ??= await _connection.CreateChannelAsync();
    }

    public async Task PublishCasinoWagerAsync(CasinoWager casinoWager)
    {
        await EnsureConnection();
        var routingKey =
            $"{_rabbitSettings.CasinoWagerRoutingKey}.{FormatString(casinoWager.CountryCode)}.{FormatString(casinoWager.Provider)}.{FormatString(casinoWager.Theme)}";

        if (routingKey.Length > 255)
        {
            _logger.LogError($"Routing key - {routingKey} is longer than 255 characters");
            throw new ArgumentException($"Routing key - {routingKey} is longer than 255 characters");
        }

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(casinoWager));
        await _channel.BasicPublishAsync(exchange: Exchange, routingKey: routingKey, body: body);
    }

    static string FormatString(string input)
    {
        // Trim the input and replace all whitespace with a single dash
        return Regex.Replace(input.Trim().ToLower(), @"\s+", "-");
    }
}