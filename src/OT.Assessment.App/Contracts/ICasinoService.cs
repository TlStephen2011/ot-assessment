using OT.Assessment.Domain.Helpers;
using OT.Assessment.Domain.Models.Dtos;

namespace OT.Assessment.App.Contracts;

public interface ICasinoService
{
    Task<PagedResponse<CasinoWagersResponseDto>> GetLatestCasinoWagersAsync(Guid playerId, int pageSize, int page);
    Task<IEnumerable<TopSpendersResponseDto>> GetTopSpenders(int count);
}