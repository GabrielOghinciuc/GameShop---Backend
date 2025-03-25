using System.ComponentModel.DataAnnotations;

namespace Gamestore.Validations
{
    public class FirstLetterUppercaseAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var firstLetterChar = value.ToString()![0];
            var firstLetter = firstLetterChar.ToString();

            if (firstLetterChar != char.ToUpper(firstLetterChar))
            {
                return new ValidationResult("The first letter should be uppercase");
            }

            return ValidationResult.Success;
        }
    }
}