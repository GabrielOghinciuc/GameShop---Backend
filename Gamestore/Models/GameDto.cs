using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gamestore.Validations;
using Microsoft.AspNetCore.Http;

namespace Gamestore.Models
{
    [Table("Games")]
    public class GameDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [FirstLetterUppercase]
        [StringLength(100)]
        public string Name { get; set; }

        [FirstLetterUppercase]
        public string? Description { get; set; }

        [Required]
        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? Picture { get; set; }


        [Required]
        [Range(0, double.MaxValue)]
        public double OriginalPrice { get; set; }

        [ValidateDiscountPrice]
        public double? DiscountedPrice { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; }

        public bool showFullDescription { get; set; }

        public bool showOnFirstPage { get; set; }

        public List<string> Platforms { get; set; } = new List<string>();

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }
    }
}
