using AirShop.Web.Authorization;
using AirShop.Web.DTOs;
using AirShop.Web.Models;
using AirShop.Web.Repositories;
using AirShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Весь контроллер требует JWT
public class ReservationsApiController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IAuthorizationService _authorizationService;
    private readonly IGenericRepository<ProductReservation> _reservationRepo;

    public ReservationsApiController(
        IReservationService service,
        IAuthorizationService authorizationService,
        IGenericRepository<ProductReservation> reservationRepo)
    {
        _service = service;
        _authorizationService = authorizationService;
        _reservationRepo = reservationRepo;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve(CreateReservationDto dto)
    {
        var result = await _service.ReserveAsync(dto);
        if (result.IsNotFound) return NotFound(new { error = result.ErrorMessage });
        if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _reservationRepo.Query().FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null) return NotFound();

        // ПРОВЕРКА RESOURCE-BASED POLICY
        var authResult = await _authorizationService.AuthorizeAsync(User, reservation, new ReservationOwnerRequirement());
        if (!authResult.Succeeded) return Forbid(); // 403 Forbidden

        var result = await _service.CancelReservationAsync(id);
        if (result.IsNotFound) return NotFound(new { error = result.ErrorMessage });
        if (!result.IsSuccess) return BadRequest(new { error = result.ErrorMessage });

        return NoContent();
    }
}