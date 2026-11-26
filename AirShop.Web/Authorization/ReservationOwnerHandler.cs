using AirShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AirShop.Web.Authorization;

public class ReservationOwnerHandler : AuthorizationHandler<ReservationOwnerRequirement, ProductReservation>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ReservationOwnerRequirement requirement, ProductReservation resource)
    {
        // Если пользователь Админ - доступ разрешен
        if (context.User.IsInRole("admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Иначе проверяем, совпадает ли ID текущего пользователя с ID владельца резерва
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int currentUserId) && resource.UserId == currentUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}