using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Components.Web.RenderMode;
namespace Service.UI.CMS.Blazor.Components;
public partial class App
{
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    private IComponentRenderMode? PageRenderMode => HttpContext.AcceptsInteractiveRouting() ? InteractiveServer : null;
}
