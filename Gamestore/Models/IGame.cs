namespace Gamestore.Models
{
    public interface IGame
    {
        int Id { get; set; }
        string Name { get; set; }
        string? Description { get; set; }
        string Image { get; set; }
        double OriginalPrice { get; set; }
        double? DiscountedPrice { get; set; }
        double Rating { get; set; }
        bool showFullDescription { get; set; }
        bool showOnFirstPage { get; set; }
        List<string> Platforms { get; set; }
        string Genre { get; set; }
    }
}
