using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace Service.TanAn.Infrastructure.Services;
/// <summary>Execution-local identity shared by nested Blazor scopes; HTTP fallback for API/SSR.</summary>
public sealed class DataActor(IHttpContextAccessor http)
{
    private readonly AsyncLocal<ClaimsPrincipal?> current = new();
    public ClaimsPrincipal? Principal { get => current.Value ?? http.HttpContext?.User; set => current.Value = value; }
    public Guid UserId => Principal?.Identity?.IsAuthenticated == true && Guid.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
}
