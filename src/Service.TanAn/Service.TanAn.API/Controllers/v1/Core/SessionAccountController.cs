using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.API.Authentication;
using Service.TanAn.Application.Services.Core;

namespace Service.TanAn.API.Controllers.v1.Core;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/session-account")]
[Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
public sealed class SessionAccountController(AccountPasswordService passwords) : ControllerBase
{
    [HttpPut("password")]
    public async Task<ActionResult<ResultAPI<bool>>> ChangePassword([FromBody] ChangeOwnPasswordForm form)
    {
        var actor = new CurrentUserDto
        {
            UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), UserName = User.Identity!.Name!,
            Role = User.FindFirstValue(ClaimTypes.Role)!, IsAuthenticated = true
        };
        try { await passwords.ChangeOwnPasswordAsync(form, actor); return Ok(new ResultAPI<bool> { Data = true }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new ResultAPI<bool> { Success = false, Status = Service.Shared.Commons.Interfaces.StatusCode.BadRequest, Message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new ResultAPI<bool> { Success = false, Status = Service.Shared.Commons.Interfaces.StatusCode.NotFound, Message = ex.Message }); }
    }
}
