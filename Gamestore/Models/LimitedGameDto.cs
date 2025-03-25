namespace Gamestore.Models
{
    // Renamed class to be more descriptive but keeping the same structure
    public class LimitedGameDto
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public double OriginalPrice { get; set; }
        public double? DiscountedPrice { get; set; }
        public List<string> Platforms { get; set; } = new List<string>();
    }
}
