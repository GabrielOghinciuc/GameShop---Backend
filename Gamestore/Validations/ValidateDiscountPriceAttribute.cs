using System.ComponentModel.DataAnnotations;
using Gamestore.Models;

namespace Gamestore.Validations
{
    public class ValidateDiscountPriceAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var game = validationContext.ObjectInstance as GameDto;
            
            if (game is null || value is null)
            {
                return ValidationResult.Success;
            }

            double? discountedPrice = (double?)value;
            
            if (discountedPrice > game.OriginalPrice)
            {
                return new ValidationResult("Discounted price cannot be greater than the original price");
            }

            return ValidationResult.Success;
        }
    }
}
