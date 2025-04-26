namespace Gamestore.Models;

public class GetGamesByIdsRequest
{
    public List<int> GameIds { get; set; } = new();
}
