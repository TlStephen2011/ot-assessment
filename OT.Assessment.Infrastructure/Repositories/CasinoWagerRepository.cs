using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OT.Assessment.Domain.Helpers;
using OT.Assessment.Domain.Models;
using OT.Assessment.Domain.Models.Dtos;

namespace OT.Assessment.Infrastructure.Repositories;

public class CasinoWagerRepository(IConfiguration configuration) : ICasinoWagerRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DatabaseConnection")!;

    public async Task CreateCasinoWager(CasinoWager wager)
    {
        await using var connection = new SqlConnection(_connectionString);
        connection.Open();

        await connection.ExecuteAsync(
            "InsertCasinoWager",
            new
            {
                WagerId = wager.WagerId,
                ProviderName = wager.Provider,
                ThemeName = wager.Theme,
                Amount = wager.Amount,
                PlayerAccountId = wager.AccountId,
                PlayerUsername = wager.Username,
                CountryCode = wager.CountryCode,
                CreatedDateTime = wager.CreatedDateTime,
                GameName = wager.GameName
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<PagedResponse<CasinoWagersResponseDto>> GetCasinoWagersAsync(Guid playerId, int pageSize, int page)
    {
        var query = @"
            SELECT 
                Wager.Id as PrimaryWagerId, Wager.WagerId, Wager.Amount, Wager.CreatedDateTime, 
                Game.Id as GameId, Game.Name, 
                Provider.Id as ProviderId, Provider.Name
            FROM Wager
            JOIN Player ON Wager.PlayerId = Player.Id
            JOIN Provider ON Wager.ProviderId = Provider.Id
            JOIN Game ON Wager.GameId = Game.Id
            WHERE Player.AccountId = @PlayerId
            ORDER BY Wager.CreatedDateTime DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
        ";

        var countQuery = @"
            SELECT COUNT(1) 
            FROM Wager
            JOIN Player ON Wager.PlayerId = Player.Id
            WHERE Player.AccountId = @PlayerId
        ";

        await using var connection = new SqlConnection(_connectionString);

        var count = await connection.ExecuteScalarAsync<int>(countQuery, new { PlayerId = playerId });

        int offset = (page - 1) * pageSize;

        var casinoWagersResponse = new List<CasinoWagersResponseDto>();

        await connection.QueryAsync<Wager, Provider, Game, Wager>(
            query,
            (wager, provider, game) =>
            {
                casinoWagersResponse.Add(new CasinoWagersResponseDto
                {
                    CreatedDate = wager.CreatedDateTime,
                    Provider = provider.Name,
                    Amount = wager.Amount,
                    Game = game.Name,
                    WagerId = wager.WagerId
                });

                return wager;
            },
            new
            {
                PlayerId = playerId,
                PageSize = pageSize,
                Offset = offset
            },
            splitOn: "GameId,ProviderId"
        );

        return new PagedResponse<CasinoWagersResponseDto>
        {
            Data = casinoWagersResponse,
            Total = count,
            PageSize = pageSize,
            Page = page
        };
    }

    public async Task<IEnumerable<TopSpendersResponseDto>> GetTopSpenders(int count)
    {
        var query = @"
            SELECT Wager.Id, Wager.Amount, Player.Id, Player.AccountId, Player.Username 
            FROM Wager
            JOIN Player ON Wager.PlayerId = Player.Id
            ORDER BY Wager.Amount DESC
            OFFSET 0 ROWS
            FETCH NEXT @Count ROWS ONLY
        ";

        await using var connection = new SqlConnection(_connectionString);

        var topSpendersResponse = new List<TopSpendersResponseDto>();

        await connection.QueryAsync<Wager, Player, Wager>(query, (wager, player) =>
        {
            topSpendersResponse.Add(new TopSpendersResponseDto
            {
                AccountId = player.AccountId,
                Username = player.Username,
                TotalAmountSpent = wager.Amount
            });

            return wager;
        },
        new
        {
            Count = count
        });

        return topSpendersResponse;
    }
}