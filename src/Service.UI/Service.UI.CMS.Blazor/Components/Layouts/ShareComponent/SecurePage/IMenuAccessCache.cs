using Service.Shared.Commons.Models;

namespace Services.Components.Layouts.ShareComponent.SecurePage
{
    public interface IMenuAccessCache
    {
        bool HasMenu(CurrentUserDto user, Guid menuId);
        void Invalidate();
    }
}

namespace Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage
{
    public interface IMenuAccessCache : Services.Components.Layouts.ShareComponent.SecurePage.IMenuAccessCache
    {
    }
}
