using Akka.Hosting;
using Akka.Streams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OT.Assessment.Consumer.Consumers;
using OT.Assessment.Domain.Configuration.Options;

namespace OT.Assessment.Consumer;

public static class AkkaRegistrationExtensions
{
    public static IServiceCollection AddAkkaClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAkka("AssessmentActorSystem", (builder, sp) =>
        {
            builder.WithActors((system, _, _) =>
            {
                system.ActorOf(PlayerWagerConsumer.Props(sp), "PlayerWagerConsumer");
            });
        });

        return services;
    }
}