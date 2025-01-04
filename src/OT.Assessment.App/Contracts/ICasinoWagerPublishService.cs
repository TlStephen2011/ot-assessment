using OT.Assessment.Domain.Models;

namespace OT.Assessment.App.Contracts;

public interface ICasinoWagerPublishService
{
    Task PublishCasinoWagerAsync(CasinoWager casinoWager);
}