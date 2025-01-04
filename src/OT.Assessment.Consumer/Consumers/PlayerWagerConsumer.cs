using System.Text.Json;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Amqp.RabbitMq;
using Akka.Streams.Amqp.RabbitMq.Dsl;
using Akka.Streams.Dsl;
using Microsoft.Extensions.Options;
using OT.Assessment.Domain.Configuration.Options;
using OT.Assessment.Domain.Models;
using OT.Assessment.Infrastructure.Repositories;
using RabbitMQ.Client.Exceptions;

namespace OT.Assessment.Consumer.Consumers;

internal class PlayerWagerConsumer : ReceiveActor
{
    private readonly ICasinoWagerRepository _casinoWagerRepository;
    private readonly Source<IncomingMessage, NotUsed> _amqpSource;
    private readonly IServiceScope _scope;
    private readonly ILogger<PlayerWagerConsumer> _logger;

    public static Props Props(IServiceProvider sp) => Akka.Actor.Props.Create(() => new PlayerWagerConsumer(sp));

    public PlayerWagerConsumer(IServiceProvider sp)
    {
        _scope = sp.CreateScope();

        var rabbitSettings = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        _casinoWagerRepository = sp.GetRequiredService<ICasinoWagerRepository>();

        var connectionDetails = AmqpConnectionDetails
            .Create(rabbitSettings.Host, rabbitSettings.Port)
            .WithCredentials(AmqpCredentials.Create(rabbitSettings.Username, rabbitSettings.Password));

        var queueDeclaration = QueueDeclaration
            .Create(rabbitSettings.CasinoWagerRoutingKey)
            .WithDurable(true)
            .WithAutoDelete(false);

        _logger = sp.GetRequiredService<ILogger<PlayerWagerConsumer>>();
        
        var exchange = "AssessmentExchange";

        var binding = BindingDeclaration
            .Create(rabbitSettings.CasinoWagerRoutingKey, exchange)
            .WithRoutingKey($"{rabbitSettings.CasinoWagerRoutingKey}.*.*.*");

        _amqpSource = RestartSource.OnFailuresWithBackoff(() => AmqpSource
            .AtMostOnceSource(NamedQueueSourceSettings
                .Create(connectionDetails, rabbitSettings.CasinoWagerRoutingKey)
                .WithConsumerTag(Self.Path.Name)
                .WithDeclarations(queueDeclaration, binding), bufferSize: 100), 
            RestartSettings.Create(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), 0.2));
    }

    protected override void PreStart()
    {
        base.PreStart();

        var source = _amqpSource
            .TakeWhile(_ => true, inclusive: true) // Keeps consuming until a condition is met
            .Select(item => JsonSerializer.Deserialize<CasinoWager>(item.Bytes.ToArray())) // Deserialize the incoming message
            .GroupedWithin(100, TimeSpan.FromMilliseconds(100)); // Group messages for batch processing

        // Define the flow
        var flow = Flow.Create<IEnumerable<CasinoWager>>()
            .SelectAsync(20,async items =>
            {
                foreach (var casinoWager in items)
                {
                    _logger.LogInformation("Received message for wagerId - {WagerId} and account - {AccountId}", casinoWager.WagerId, casinoWager.AccountId);
                    await _casinoWagerRepository.CreateCasinoWager(casinoWager);
                }

                return Done.Instance;
            });

        // Define the sink
        var sink = Sink.Ignore<Done>();

        source.Via(flow).To(sink).Run(Context.System.Materializer());
    }


    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 5,
            withinTimeMilliseconds: 20_000,
            localOnlyDecider: ex =>
            {
                if (ex is BrokerUnreachableException)
                    return Directive.Restart;
                return Directive.Resume;
            }
        );
    }

    protected override void PostStop()
    {
        _scope.Dispose();
        base.PostStop();
    }
}