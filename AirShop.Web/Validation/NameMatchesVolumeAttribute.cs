using AirShop.Web.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AirShop.Web.Validation;

public class NameMatchesVolumeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var product = (Product)validationContext.ObjectInstance;

        if (value is decimal volume && !string.IsNullOrEmpty(product.Name))
        {
            // Ищем в названии число с точкой или запятой перед буквой "л" (например, "1 л" или "0.5 л")
            var match = Regex.Match(product.Name, @"(\d+([\.,]\d+)?)\s*л");
            if (match.Success)
            {
                var nameVolumeStr = match.Groups[1].Value.Replace(',', '.');
                if (decimal.TryParse(nameVolumeStr, System.Globalization.CultureInfo.InvariantCulture, out decimal nameVolume))
                {
                    if (nameVolume != volume)
                    {
                        return new ValidationResult($"Объем в названии ({nameVolume} л) не совпадает с указанным объемом ({volume} л)");
                    }
                }
            }
        }
        return ValidationResult.Success;
    }
}