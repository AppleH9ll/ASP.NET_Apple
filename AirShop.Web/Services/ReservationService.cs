using AirShop.Web.DTOs;
using AirShop.Web.Models;
using AirShop.Web.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Services;

public interface IReservationService
{
    Task<Result<ReservationDto>> ReserveAsync(CreateReservationDto dto);
    Task<Result<bool>> CancelReservationAsync(int reservationId);
}

public class ReservationService : IReservationService
{
    private readonly IGenericRepository<Product> _productRepo;
    private readonly IGenericRepository<ProductReservation> _reservationRepo;
    private readonly IMapper _mapper;

    public ReservationService(
        IGenericRepository<Product> productRepo,
        IGenericRepository<ProductReservation> reservationRepo,
        IMapper mapper)
    {
        _productRepo = productRepo;
        _reservationRepo = reservationRepo;
        _mapper = mapper;
    }

    // ОПЕРАЦИЯ 1: Резервирование (уменьшение остатка + создание записи)
    public async Task<Result<ReservationDto>> ReserveAsync(CreateReservationDto dto)
    {
        // GetByIdAsync возвращает сущность, которую EF Core сразу начинает отслеживать
        var product = await _productRepo.GetByIdAsync(dto.ProductId);
        if (product == null)
            return Result<ReservationDto>.NotFound("Товар не найден");

        if (product.Stock < dto.Quantity)
            return Result<ReservationDto>.Failure($"Недостаточно товара. Доступно: {product.Stock}");

        // 1. Уменьшаем остаток. EF Core увидит это изменение при SaveChanges.
        product.Stock -= dto.Quantity;

        // 2. Создаем запись о резерве
        var reservation = new ProductReservation
        {
            ProductId = dto.ProductId,
            UserId = dto.UserId,
            Quantity = dto.Quantity,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };
        await _reservationRepo.AddAsync(reservation);

        // 3. ОДИН вызов SaveChangesAsync для обеих сущностей!
        await _reservationRepo.SaveChangesAsync();

        return Result<ReservationDto>.Success(_mapper.Map<ReservationDto>(reservation));
    }

    // ОПЕРАЦИЯ 2: Отмена (возврат остатка + закрытие записи)
    public async Task<Result<bool>> CancelReservationAsync(int reservationId)
    {
        // Загружаем резерв вместе с продуктом. Обе сущности начинают отслеживаться.
        var reservation = await _reservationRepo.Query()
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null || reservation.Status != "active")
            return Result<bool>.NotFound("Активная запись о резервировании не найдена");

        // 1. Возвращаем товар на склад
        if (reservation.Product != null)
        {
            reservation.Product.Stock += reservation.Quantity;
        }

        // 2. Закрываем резерв
        reservation.Status = "cancelled";

        // 3. ОДИН вызов SaveChangesAsync. EF Core сам поймет, что изменились две таблицы.
        await _reservationRepo.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}