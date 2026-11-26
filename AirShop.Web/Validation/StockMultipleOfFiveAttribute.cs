using System.ComponentModel.DataAnnotations;

namespace AirShop.Web.Validation;

public class StockMultipleOfFiveAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int stock)
        {
            if (stock <= 0)
            {
                return new ValidationResult("Остаток должен быть больше нуля");
            }
            if (stock % 5 != 0)
            {
                return new ValidationResult("Количество на складе должно быть кратно 5 (мы фасуем воздух в коробки по 5 баллонов)");
            }
        }
        return ValidationResult.Success;
    }
}