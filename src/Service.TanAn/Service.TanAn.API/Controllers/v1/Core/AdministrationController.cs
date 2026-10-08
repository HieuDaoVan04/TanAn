using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.API.Authentication;
using Service.TanAn.Application.Services.Core;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.API.Controllers.v1.Core;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/administration")]
[Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName, Roles = "Admin")]
public sealed class AdministrationController(AdministrationService service, TanAnDbContext db) : ControllerBase
{
    private CurrentUserDto Actor => new() {
        UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
        UserName = User.Identity!.Name!, Role = User.FindFirstValue(ClaimTypes.Role)!, IsAuthenticated = true
    };

    [HttpGet("villages")]
    public async Task<ActionResult<ResultAPI<List<AdministrationVillageDto>>>> Villages()
        => Ok(new ResultAPI<List<AdministrationVillageDto>> { Data = await db.ApThons.AsNoTracking()
            .Where(x => x.DangHoatDong && x.XaId != null).OrderBy(x => x.Ten)
            .Select(x => new AdministrationVillageDto { Id = x.Id, Ten = x.Ten }).ToListAsync() });

    [HttpGet("{catalog}")]
    public Task<ActionResult<ResultAPI<List<AdminRecord>>>> List(string catalog)
        => Execute(async () => await service.ListAsync(ParseCatalog(catalog), Actor));

    [HttpPost("{catalog}")]
    public Task<ActionResult<ResultAPI<bool>>> Save(string catalog, [FromBody] AdminRecord form)
        => Execute(async () => { await service.SaveAsync(ParseCatalog(catalog), form, Actor); return true; });

    [HttpDelete("{catalog}/{id:guid}")]
    public Task<ActionResult<ResultAPI<bool>>> Delete(string catalog, Guid id)
        => Execute(async () => { await service.DeleteAsync(ParseCatalog(catalog), id, Actor); return true; });

    private static AdminCatalog ParseCatalog(string value) => value.ToLowerInvariant() switch {
        "users" => AdminCatalog.Users, "roles" => AdminCatalog.Roles,
        "groups" => AdminCatalog.Groups, "menus" => AdminCatalog.Menus,
        "modules" => AdminCatalog.Modules, "parameters" => AdminCatalog.Parameters,
        _ => throw new KeyNotFoundException("Chức năng quản trị không tồn tại.")
    };

    private async Task<ActionResult<ResultAPI<T>>> Execute<T>(Func<Task<T>> action)
    {
        try { return Ok(new ResultAPI<T> { Data = await action() }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (KeyNotFoundException ex) { return NotFound(new ResultAPI<T> { Success = false, Status = Service.Shared.Commons.Interfaces.StatusCode.NotFound, Message = ex.Message }); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { return BadRequest(new ResultAPI<T> { Success = false, Status = Service.Shared.Commons.Interfaces.StatusCode.BadRequest, Message = ex.Message }); }
    }
}
