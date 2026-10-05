// "Một sản phẩm của HieuDV"

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;

namespace Service.TanAn.Infrastructure.Services
{
    public class RequestContext : IRequestContext
    {
        public CurrentUserDto CurrentUser { get; set; } = new CurrentUserDto();

        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                CurrentUser.IsAuthenticated = true;
                CurrentUser.Username = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                CurrentUser.FullName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? CurrentUser.Username;
                CurrentUser.Role = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(idClaim) && System.Guid.TryParse(idClaim, out var userId))
                {
                    CurrentUser.UserId = userId;
                }
            }
        }
    }
}


