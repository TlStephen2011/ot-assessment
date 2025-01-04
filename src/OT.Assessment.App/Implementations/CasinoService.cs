using OT.Assessment.App.Contracts;
using OT.Assessment.Domain.Helpers;
using OT.Assessment.Domain.Models;
using OT.Assessment.Domain.Models.Dtos;
using OT.Assessment.Infrastructure.Repositories;

namespace OT.Assessment.App.Implementations;

public class CasinoService(ICasinoWagerRepository casinoWagerRepository, ILogger<CasinoService> logger) : ICasinoService
{
    public async Task<PagedResponse<CasinoWagersResponseDto>> GetLatestCasinoWagersAsync(Guid playerId, int pageSize, int page)
    {
        try
        {
            var latestWagers = await casinoWagerRepository.GetCasinoWagersAsync(playerId, pageSize, page);
            return latestWagers;
        }
        catch (Exception e)
        {
            logger.LogError(e,"Unable to fetch latest wagers for {PlayerId}", playerId);
            return null;
        }
    }

    public async Task<IEnumerable<TopSpendersResponseDto>> GetTopSpenders(int count)
    {
        try
        {
            var topSpenders = await casinoWagerRepository.GetTopSpenders(count);
            return topSpenders;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to fetch top spenders");
            return null;
        }
    }
}