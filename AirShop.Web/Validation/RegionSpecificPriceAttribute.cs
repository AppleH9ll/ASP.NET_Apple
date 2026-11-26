using AirShop.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace AirShop.Web.Validation;

public class RegionSpecificPriceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var product = (Product)validationContext.ObjectInstance;

        // Если регион содержит "Россия", то цена не может быть слишком высокой
        if (value is decimal price && product.OriginRegion != null && product.OriginRegion.Contains("Россия"))
        {
            if (price > 3000)
            {
                return new ValidationResult("Отечественный воздух не может стоить дороже 3 000 ₽ (ограничение импортозамещения)");
            }
        }
        return ValidationResult.Success;
    }
}