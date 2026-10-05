using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Service.TanAn.Infrastructure.Services;
namespace Service.UI.CMS.Blazor.Applications;
public sealed class VillageCircuitHandler(DataActor actor, AuthenticationStateProvider authentication) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(Func<CircuitInboundActivityContext, Task> next)
        => async context => {
            var previous = actor.Principal;
            try { actor.Principal = (await authentication.GetAuthenticationStateAsync()).User; await next(context); }
            finally { actor.Principal = previous; }
        };
}
