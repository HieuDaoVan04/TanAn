using Service.Shared.Commons.Models;
using Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage;

void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
    Console.WriteLine($"PASS: {description}");
}

var householdMenuId = Guid.NewGuid();
var welfareMenuId = Guid.NewGuid();
var cache = new MenuAccessCache();
var user = new CurrentUserDto
{
    UserId = Guid.NewGuid(), IsAuthenticated = true,
    Menus = new() { new() { Id = householdMenuId } }
};
Check(cache.HasMenu(user, householdMenuId), "existing Menus feeds MenusActive");
Check(!cache.HasMenu(user, welfareMenuId), "ungranted menu denied");
Check(!cache.HasMenu(user, Guid.Empty), "empty menu ID denied");
var other = new CurrentUserDto { UserId = Guid.NewGuid(), IsAuthenticated = true };
Check(!cache.HasMenu(other, householdMenuId), "account switch cannot reuse previous grants");
Check(cache.HasMenu(user, householdMenuId), "returning account rebuilds cache");
user.IsAuthenticated = false;
Check(!cache.HasMenu(user, householdMenuId), "logout denies cached grant");
user.Menus.Clear();
user.IsAuthenticated = true;
Check(!cache.HasMenu(user, householdMenuId), "relogin does not restore revoked grant");
user.MenusActive = new() { new() { Id = welfareMenuId } };
Check(cache.HasMenu(user, welfareMenuId) && !cache.HasMenu(user, householdMenuId), "replacement grants refresh same account");
user.MenusActive.Clear();
cache.Invalidate();
Check(!cache.HasMenu(user, welfareMenuId), "in-place revocation followed by invalidation");
user.Role = "Admin";
Check(!cache.HasMenu(user, householdMenuId), "role name does not bypass grants");
user.MenusActive = new() { new() { Id = householdMenuId, Path = "/ho-khau" } };
Check(MenuSecure.FindMenuId(user, "/ho-khau") == householdMenuId, "lookup uses stored ID");
Check(MenuSecure.FindMenuId(user, "/missing") == null, "missing path never invents an ID");
Console.WriteLine("All menu access checks passed.");