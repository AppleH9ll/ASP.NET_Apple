using Microsoft.AspNetCore.Authorization;

namespace AirShop.Web.Authorization;

// Требование: пользователь должен быть владельцем ресурса
public class ReservationOwnerRequirement : IAuthorizationRequirement { }