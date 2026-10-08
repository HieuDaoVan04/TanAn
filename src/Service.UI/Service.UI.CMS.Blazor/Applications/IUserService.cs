using Microsoft.AspNetCore.Components.Authorization;
using Service.Shared.Commons.Models;

namespace Service.UI.CMS.Blazor.Applications;

public interface IUserService
{
    Task<CurrentUserDto> GetCurrentUserAsync();
}

public sealed class UserService(AuthenticationStateProvider authentication, AccountService accounts) : IUserService
{
    public async Task<CurrentUserDto> GetCurrentUserAsync()
        => await accounts.GetCurrentAsync((await authentication.GetAuthenticationStateAsync()).User);
}
