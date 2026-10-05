using Service.Shared.Commons.Models;

namespace Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage
{
    /// <summary>Tra ID đã lưu theo đường dẫn; không tự đặt hoặc sinh GUID ở phía giao diện.</summary>
    public static class MenuSecure
    {
        public static readonly Guid QuanLyDonViHeThong = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public static Guid? FindMenuId(CurrentUserDto user, string path) =>
            user.MenusActive?.FirstOrDefault(x => string.Equals(x.Path, path, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}

namespace Service.UI.Blazor.Components.Pages.QuanTriHeThong.Menu
{
    public static class MenuSecure
    {
        public static readonly Guid QuanLyDonViHeThong = Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage.MenuSecure.QuanLyDonViHeThong;
    }
}
