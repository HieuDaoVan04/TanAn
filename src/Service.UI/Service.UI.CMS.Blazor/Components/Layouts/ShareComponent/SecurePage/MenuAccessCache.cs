using Service.Shared.Commons.Models;

namespace Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage;

/// <summary>
/// Cache quyền hiển thị menu trong DI scope của Blazor. Không thay thế quyền tại API.
/// Khi sửa danh sách quyền tại chỗ, gọi Invalidate trước khi kiểm tra lại.
/// </summary>
public sealed class MenuAccessCache : IMenuAccessCache
{
    private HashSet<Guid>? _menuIds;
    private Guid _builtForUser;
    private List<MenuItemDto>? _builtFromMenus;

    public bool HasMenu(CurrentUserDto user, Guid menuId)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!user.IsAuthenticated || user.UserId == Guid.Empty)
        {
            Invalidate();
            return false;
        }

        if (menuId == Guid.Empty) return false;

        if (_menuIds is null || _builtForUser != user.UserId
            || !ReferenceEquals(_builtFromMenus, user.MenusActive))
        {
            _builtFromMenus = user.MenusActive;
            _menuIds = _builtFromMenus is null
                ? new HashSet<Guid>()
                : _builtFromMenus.Select(m => m.Id).ToHashSet();
            _builtForUser = user.UserId;
        }

        return _menuIds.Contains(menuId);
    }

    public void Invalidate()
    {
        _menuIds = null;
        _builtFromMenus = null;
        _builtForUser = Guid.Empty;
    }
}
