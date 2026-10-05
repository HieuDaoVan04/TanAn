using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.API.Authentication;
using Service.TanAn.Application.Services.Core;

namespace Service.TanAn.API.Controllers.v1.Core;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/system-configuration")]
[Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName, Roles = "Admin")]
public sealed class SystemConfigurationController(SystemConfigurationService service) : ControllerBase
{
    private CurrentUserDto Actor => new()
    {
        UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), UserName = User.Identity!.Name!,
        Role = User.FindFirstValue(ClaimTypes.Role)!, IsAuthenticated = true
    };

    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<ActionResult<ResultAPI<PublicSystemConfigurationDto>>> Public()
        => Ok(new ResultAPI<PublicSystemConfigurationDto> { Data = await service.GetPublicAsync() });

    [AllowAnonymous]
    [HttpGet("password-policy")]
    public async Task<ActionResult<ResultAPI<PasswordPolicyDto>>> PasswordPolicy()
        => Ok(new ResultAPI<PasswordPolicyDto> { Data = await service.GetPasswordPolicyAsync() });

    [HttpGet("general")]
    public async Task<ActionResult<ResultAPI<List<SystemConfigurationField>>>> General()
        => Ok(new ResultAPI<List<SystemConfigurationField>> { Data = await service.GetGeneralAsync(Actor) });

    [HttpPut("general")]
    public Task<ActionResult<ResultAPI<bool>>> Save([FromBody] List<SystemConfigurationUpdate> updates)
        => Execute(async () => await service.SaveGeneralAsync(updates, Actor));

    [HttpPost("sync")]
    public Task<ActionResult<ResultAPI<bool>>> Sync() => Execute(async () => await service.SyncDefinitionsAsync(Actor));

    private async Task<ActionResult<ResultAPI<bool>>> Execute(Func<Task> action)
    {
        try { await action(); return Ok(new ResultAPI<bool> { Data = true }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { return BadRequest(new ResultAPI<bool> { Success = false, Status = Service.Shared.Commons.Interfaces.StatusCode.BadRequest, Message = ex.Message }); }
    }
}
