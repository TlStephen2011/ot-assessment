using OT.Assessment.Domain.Helpers;
using OT.Assessment.Domain.Models;
using OT.Assessment.Domain.Models.Dtos;

namespace OT.Assessment.Infrastructure.Repositories;

public interface ICasinoWagerRepository
{
    Task CreateCasinoWager(CasinoWager wager);
    Task<PagedResponse<CasinoWagersResponseDto>> GetCasinoWagersAsync(Guid playerId, int pageSize, int page);
    Task<IEnumerable<TopSpendersResponseDto>> GetTopSpenders(int count);
}