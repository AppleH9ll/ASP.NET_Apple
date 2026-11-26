using AirShop.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace AirShop.Web.Validation;

public class PremiumPriceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var product = (Product)validationContext.ObjectInstance;

        if (value is decimal price && !string.IsNullOrEmpty(product.Name))
        {
            if (product.Name.ToLower().Contains("премиум") && price < 10000)
            {
                return new ValidationResult("Премиальный воздух не может стоить дешевле 10 000 ₽");
            }
            if (product.Name.ToLower().Contains("эконом") && price > 1500)
            {
                return new ValidationResult("Эконом-воздух не может стоить дороже 1 500 ₽");
            }
        }
        return ValidationResult.Success;
    }
}