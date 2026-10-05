// "Một sản phẩm của HieuDV"

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Interfaces;
using Service.TanAn.Application;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Infrastructure;
using Service.TanAn.Infrastructure.Persistence;
using Service.UI.CMS.Blazor.Applications;
using Service.UI.CMS.Blazor.Components;
using Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage;

// Enable Legacy Timestamp behavior for Npgsql PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add Blazor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add FluentUI
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, VillageCircuitHandler>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SessionAuthenticationStateProvider>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "TanAn.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = async context =>
    {
        try
        {
            var current = await context.HttpContext.RequestServices.GetRequiredService<AccountService>().GetCurrentAsync(context.Principal!);
            if (current.IsAuthenticated && context.Principal!.IsInRole("Admin") == (current.Role == "Admin")) return;
        }
        catch { /* Redis không khả dụng: không chấp nhận phiên chưa được xác minh. */ }
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    };
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// Add UI State & Module Registries & User Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<ModuleTypeState>();
builder.Services.AddScoped<SystemConfigurationState>();
builder.Services.AddScoped<ICallServiceRegistry, CallServiceRegistry>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMenuAccessCache, MenuAccessCache>();
builder.Services.AddScoped<MenuTreeService>();
builder.Services.AddScoped<ApiServiceTransport>();
builder.Services.AddHttpClient("ServiceTanAn").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
builder.Services.AddScoped<SessionAdministrationService>();
builder.Services.AddScoped<Service.Shared.Commons.Interfaces.Extentions.IPreviewTokenService, Service.Shared.Commons.Services.PreviewTokenService>();
builder.Services.AddScoped<Service.UI.CMS.Blazor.Components.Layout.Component.Attachments.IFileActionService, Service.UI.CMS.Blazor.Components.Layout.Component.Attachments.FileActionService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Add DbContext (Supports both PostgreSQL and SQLite)
var connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tan_an_db.sqlite";
builder.Services.AddDbContext<TanAnDbContext>(options =>
{
    if (connString.Contains("Host=") || connString.Contains("Server=") || builder.Configuration["DatabaseProvider"]?.ToLower() == "postgresql")
    {
        options.UseNpgsql(connString);
    }
    else
    {
        options.UseSqlite(connString);
    }
});

// Add Application & Infrastructure Services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();

app.MapPost("/account/sign-in", async (HttpContext context, IAntiforgery antiforgery, AccountService accounts) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest("Phiên biểu mẫu hết hạn. Hãy tải lại trang đăng nhập."); }
    var form = await context.Request.ReadFormAsync();
    var returnUrl = AccountService.IsLocalReturnUrl(form["returnUrl"]) ? form["returnUrl"].ToString() : "/ban-lam-viec";
    System.Security.Claims.ClaimsPrincipal? principal;
    try { principal = await accounts.SignInAsync(form["username"].ToString(), form["password"].ToString(), context.Connection.RemoteIpAddress?.ToString(), context.Request.Headers.UserAgent.ToString()); }
    catch { return Results.LocalRedirect("/account/login?error=unavailable"); }
    if (principal == null) return Results.LocalRedirect("/account/login?error=invalid&returnUrl=" + Uri.EscapeDataString(returnUrl));
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
        new AuthenticationProperties { ExpiresUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst("tanan_session_expires")!.Value)), IsPersistent = false });
    return Results.LocalRedirect(returnUrl);
}).AllowAnonymous().RequireRateLimiting("login");

app.MapPost("/account/sign-out", async (HttpContext context, IAntiforgery antiforgery, ILoginSessionStore sessions) =>
{
    try { await antiforgery.ValidateRequestAsync(context); }
    catch (AntiforgeryValidationException) { return Results.BadRequest(); }
    var id = context.User.FindFirst(AccountService.SessionClaim)?.Value;
    if (id != null) await sessions.RevokeAsync(id);
    await context.SignOutAsync();
    return Results.LocalRedirect("/account/login");
}).RequireAuthorization();

// Redirect before rendering the asynchronous layout to avoid a nested SSR redirect.
app.MapGet("/", () => Results.LocalRedirect("/ban-lam-viec"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Kiểm tra schema, không tự tạo tài khoản, dữ liệu nghiệp vụ hoặc menu.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
    await DatabaseSchemaValidator.ValidateAsync(db);
}
app.Run();
