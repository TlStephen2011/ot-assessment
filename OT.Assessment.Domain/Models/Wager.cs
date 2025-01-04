namespace OT.Assessment.Domain.Models;

public class Wager
{
    public int Id { get; set; }
    public Guid WagerId { get; set; }
    public int ThemeId { get; set; }
    public int ProviderId { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public int CountryId { get; set; }
    public int PlayerId { get; set; }
    public int GameId { get; set; }
}