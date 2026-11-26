using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- Обязательно добавьте для [NotMapped]
using Microsoft.AspNetCore.Http;
using AirShop.Web.Validation;

namespace AirShop.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название товара")]
    public string Name { get; set; } = null!;

    [ContainsAirQualityKeyword]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Укажите цену")]
    [PremiumPrice]
    [RegionSpecificPrice]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Укажите остаток")]
    [StockMultipleOfFive]
    public int Stock { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    [NameMatchesVolume]
    public decimal? VolumeLiters { get; set; }

    public string? OriginRegion { get; set; }

    public string? ImageUrl { get; set; } // Это поле сохраняется в БД

    // А вот это поле нужно только для формы, в БД его сохранять НЕЛЬЗЯ
    [NotMapped]
    public IFormFile? ImageFile { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}