using System.ComponentModel.DataAnnotations;

namespace Gamestore.Models
{
    public class GameReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public double Rating { get; set; }
    }
}
