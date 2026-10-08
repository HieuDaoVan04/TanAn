using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Service.UI.CMS.Blazor.Applications;

public sealed class SessionAuthenticationStateProvider(ILoggerFactory logger, IServiceScopeFactory scopes)
    : RevalidatingServerAuthenticationStateProvider(logger)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);
    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState state, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var user = await scope.ServiceProvider.GetRequiredService<AccountService>().GetCurrentAsync(state.User);
            return user.IsAuthenticated && state.User.IsInRole("Admin") == (user.Role == "Admin");
        }
        catch { return false; }
    }
}
