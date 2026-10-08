using System.Security.Claims;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Service.Shared.Commons.Helpers;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Infrastructure.Persistence;
using Service.UI.CMS.Blazor.Applications;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Services.Core;

try
{
    using var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();
    var services = new ServiceCollection();
    services.AddDbContext<TanAnDbContext>(o => o.UseSqlite(connection));
    var sessions = new TestSessions();
    services.AddSingleton<ILoginSessionStore>(sessions);
    services.AddScoped<AccountService>();
    using var provider = services.BuildServiceProvider();
    using var scope = provider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
    db.Database.EnsureCreated();
    var admin = new User { UserName = "admin-test", FullName = "Admin", PasswordHash = "test-password", Role = RoleEnum.Admin };
    var staff = new User { UserName = "staff-test", FullName = "Staff", PasswordHash = PasswordHashing.Hash("staff-password") };
    db.Users.AddRange(admin, staff);
    db.SaveChanges();
    
    var accounts = scope.ServiceProvider.GetRequiredService<AccountService>();
    void Check(bool valid, string label) { if (!valid) throw new Exception(label); Console.WriteLine("PASS " + label); }
    Check(!AccountService.IsLocalReturnUrl("//example.com") && !AccountService.IsLocalReturnUrl("/\\example.com") && AccountService.IsLocalReturnUrl("/ban-lam-viec"), "safe return URLs");
    Check(!(await accounts.GetCurrentAsync(new ClaimsPrincipal())).IsAuthenticated, "anonymous rejected");
    Check(await accounts.SignInAsync("admin-test", "wrong-password", null, null) == null, "wrong password rejected");
    var principal = await accounts.SignInAsync("admin-test", "test-password", null, null) ?? throw new Exception("Sign in failed");
    Check((await accounts.GetCurrentAsync(principal)).Role == "Admin", "database admin authenticated");
    db.ChangeTracker.Clear();
    Check(db.Users.Single(x => x.Id == admin.Id).PasswordHash.StartsWith("pbkdf2$"), "legacy password upgraded");
    var treeService = new MenuTreeService(provider.GetRequiredService<IServiceScopeFactory>());
    Check((await treeService.GetPublishedAsync()).Count == 0 && !db.Modules.Any(), "empty database stays empty when loading menu");
    Check((await accounts.GetCurrentAsync(principal)).MenusActive.Count == 0, "admin account type does not invent menu grants");
    var root = new Module { TenModule = "Nhóm do quản trị tạo", ViTri = 8 };
    var menu = new Module { TenModule = "Trang do quản trị gán", LienKet = "/ho-khau", ModuleChaId = root.Id, ViTri = 4 };
    var unpublished = new Module { TenModule = "Chưa duyệt", LienKet = "/draft", ModerationStatus = ModerationStatus.Pending };
    db.Modules.AddRange(root, menu, unpublished);
    await db.SaveChangesAsync();
    var published = await treeService.GetPublishedAsync();
    Check(published.Single().Id == root.Id && published.Single().Children.Single().Id == menu.Id, "published menu tree comes from stored parent and child IDs");
    menu.TenModule = "Tên đã chỉnh sửa";
    menu.LienKet = "/bien-dong";
    menu.ViTri = 19;
    await db.SaveChangesAsync();
    var edited = (await treeService.GetPublishedAsync()).Single().Children.Single();
    Check(edited.Id == menu.Id && edited.TenModule == menu.TenModule && edited.LienKet == "/bien-dong" && edited.ViTri == 19, "loading menus preserves stored names, URLs and ordering");
    Check(db.Modules.Count() == 3, "menu reads do not add records");
    var staffPrincipal = await accounts.SignInAsync("staff-test", "staff-password", null, null) ?? throw new Exception("Staff failed");
    Check((await accounts.GetCurrentAsync(staffPrincipal)).MenusActive.Count == 0, "unassigned user has no grants");
    var role = new Role { RoleName = "Staff", RoleCode = "CTX" };
    db.Roles.Add(role);
    db.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = staff.Id });
    
    db.RoleModules.Add(new RoleModule { RoleId = role.Id, ModuleId = menu.Id });
    await db.SaveChangesAsync();
    Check((await accounts.GetCurrentAsync(staffPrincipal)).MenusActive.Single().Id == menu.Id, "grants loaded from database");
    Check((await accounts.GetCurrentAsync(staffPrincipal)).RoleCodes.SequenceEqual(new[] { "ctx" }), "assigned approved ctx role code loaded and normalized from database");
    db.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = admin.Id });
    await db.SaveChangesAsync();
    Check((await accounts.GetCurrentAsync(principal)).MenusActive.Single().Id == menu.Id, "admin menu also follows stored role assignments");
    role.ModerationStatus = ModerationStatus.Pending;
    await db.SaveChangesAsync();
    Check((await accounts.GetCurrentAsync(staffPrincipal)).MenusActive.Count == 0, "unapproved role does not grant menus");
    Check((await accounts.GetCurrentAsync(staffPrincipal)).RoleCodes.Count == 0, "unapproved ctx role is removed from current user immediately");
    role.ModerationStatus = ModerationStatus.Approved;
    menu.ModerationStatus = ModerationStatus.Pending;
    await db.SaveChangesAsync();
    Check((await accounts.GetCurrentAsync(staffPrincipal)).MenusActive.Count == 0, "unapproved menu is not accessible through grants");
    menu.ModerationStatus = ModerationStatus.Approved;
    await db.SaveChangesAsync();
    await sessions.RevokeAsync(staffPrincipal.FindFirstValue(AccountService.SessionClaim)!);
    Check(!(await accounts.GetCurrentAsync(staffPrincipal)).IsAuthenticated, "revoked session rejected");
    var changed = db.Users.Single(x => x.Id == admin.Id);
    changed.PasswordHash = PasswordHashing.Hash("changed-password");
    await db.SaveChangesAsync();
    Check(!(await accounts.GetCurrentAsync(principal)).IsAuthenticated, "password change invalidates session");
    changed.ModerationStatus = ModerationStatus.Pending;
    await db.SaveChangesAsync();
    Check(await accounts.SignInAsync("admin-test", "changed-password", null, null) == null, "disabled user rejected");
    for (var i = 0; i < 5; i++) await accounts.SignInAsync("staff-test", "wrong", null, null);
    Check(await accounts.SignInAsync("staff-test", "staff-password", null, null) == null, "repeated failures lock account");
    var dynamicUser = new User { UserName = "dynamic-login", PasswordHash = PasswordHashing.Hash("Dynamic-Test-123") };
    db.Users.Add(dynamicUser);
    db.SystemParameters.AddRange(
        new SystemParameter { Code = "MinuteExpireToken", Value = "15" },
        new SystemParameter { Code = "KhoaTaiKhoan", Value = "3" },
        new SystemParameter { Code = "LoginLockoutMinutes", Value = "2" });
    await db.SaveChangesAsync();
    var dynamicPrincipal = await accounts.SignInAsync(dynamicUser.UserName, "Dynamic-Test-123", null, null) ?? throw new Exception("Configured login failed");
    var configuredSession = await sessions.FindAsync(dynamicPrincipal.FindFirstValue(AccountService.SessionClaim)!);
    Check(Math.Abs((configuredSession!.ExpiresAt - configuredSession.CreatedAt).TotalMinutes - 15) < 0.1, "configured session lifetime applied to new login");
    Check(DateTimeOffset.FromUnixTimeSeconds(long.Parse(dynamicPrincipal.FindFirstValue("tanan_session_expires")!)) - configuredSession.ExpiresAt < TimeSpan.FromSeconds(1),
        "cookie expiry claim matches server session expiry");
    for (var i = 0; i < 3; i++) await accounts.SignInAsync(dynamicUser.UserName, "wrong", null, null);
    db.ChangeTracker.Clear();
    var locked = await db.Users.FindAsync(dynamicUser.Id);
    Check(locked!.LockoutEnd > DateTime.UtcNow.AddMinutes(1.8) && locked.LockoutEnd < DateTime.UtcNow.AddMinutes(2.2), "configured failure threshold and lockout duration applied");
    db.SystemParameters.Single(x => x.Code == "MinuteExpireToken").Value = "bad legacy value";
    await db.SaveChangesAsync();
    Check((await SystemConfigurationService.ReadValuesAsync(db))["MinuteExpireToken"] == "60", "invalid legacy setting falls back safely");
    Console.WriteLine("All login checks passed; only in-memory database used.");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}

sealed class TestSessions : ILoginSessionStore
{
    private readonly Dictionary<string, LoginSession> items = new();
    public Task CreateAsync(LoginSession session) { items[session.Id] = session; return Task.CompletedTask; }
    public Task<LoginSession?> FindAsync(string id) => Task.FromResult(items.GetValueOrDefault(id));
    public Task<IReadOnlyList<LoginSession>> ListAsync() => Task.FromResult<IReadOnlyList<LoginSession>>(items.Values.ToList());
    public Task RevokeAsync(string id) { items.Remove(id); return Task.CompletedTask; }
}
