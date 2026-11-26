using AutoMapper;
using AirShop.Web.Models;
using AirShop.Web.DTOs;

namespace AirShop.Web.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // === Product ===
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // Настройка для PATCH: обновляем только те поля, которые не null
        CreateMap<PatchProductDto, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // === Reservation ===
        CreateMap<ProductReservation, ReservationDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
    }
}