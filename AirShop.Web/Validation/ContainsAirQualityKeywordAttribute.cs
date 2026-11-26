using System.ComponentModel.DataAnnotations;

namespace AirShop.Web.Validation;

public class ContainsAirQualityKeywordAttribute : ValidationAttribute
{
    private readonly string[] _keywords = { "кислород", "свежесть", "чистота", "аромат", "фитонциды", "хвоя", "бриз" };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string description && !string.IsNullOrWhiteSpace(description))
        {
            bool containsKeyword = _keywords.Any(k => description.ToLower().Contains(k));
            if (!containsKeyword)
            {
                return new ValidationResult($"Описание должно содержать хотя бы одно ключевое слово: {string.Join(", ", _keywords)}");
            }
        }
        return ValidationResult.Success;
    }
}