namespace AirShop.Web.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreateReservationDto
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
}