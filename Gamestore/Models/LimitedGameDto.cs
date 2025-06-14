namespace Gamestore.Models
{
    public class LimitedGameDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Image { get; set; }
        public double OriginalPrice { get; set; }
        public double? DiscountedPrice { get; set; }
        public List<string> Platforms { get; set; } = new List<string>();
        public bool showFullDescription { get; set; }
    }
}
