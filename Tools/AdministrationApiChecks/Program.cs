using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.API.Authentication;
using Service.TanAn.API.Controllers.v1.Core;
using Service.TanAn.Application.Services.Core;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Infrastructure.Persistence;
using Service.UI.CMS.Blazor.Applications;

try
{
    using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", Args = [] });
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
    var sessions = new TestSessions();
    builder.Services.AddSingleton<ILoginSessionStore>(sessions);
    builder.Services.AddDbContext<TanAnDbContext>(o => o.UseSqlite(connection));
    builder.Services.AddScoped<Service.TanAn.Application.Interfaces.ITanAnDbContext>(
        sp => sp.GetRequiredService<TanAnDbContext>());
    builder.Services.AddScoped<AdministrationService>();
    builder.Services.AddScoped<SystemConfigurationService>();
    builder.Services.AddScoped<AccountPasswordService>();
    builder.Services.AddScoped<Service.TanAn.Application.Interfaces.IAuditLogService, Service.TanAn.Application.Services.AuditLogService>();
    builder.Services.AddScoped<Service.TanAn.Application.Interfaces.IPopulationService, Service.TanAn.Application.Services.PopulationService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.Configure<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>(o => o.XmlRepository = new TestKeyRepository());
    builder.Services.AddAuthentication(LoginSessionAuthenticationHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, LoginSessionAuthenticationHandler>(LoginSessionAuthenticationHandler.SchemeName, _ => { });
    builder.Services.AddAuthorization();
    builder.Services.AddControllers().AddApplicationPart(typeof(AdministrationController).Assembly);
    builder.Services.AddApiVersioning(o => { o.DefaultApiVersion = new ApiVersion(1, 0); o.AssumeDefaultVersionWhenUnspecified = true; });
    await using var app = builder.Build();
    app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
    var admin = new User { UserName = "admin-api-test", FullName = "API test admin", PasswordHash = "fixture-hash", Role = RoleEnum.Admin };
    var staff = new User { UserName = "staff-api-test", FullName = "API test staff", PasswordHash = "fixture-hash", Role = RoleEnum.CanBoXa };
    var chairman = new User { UserName = "chairman-api-test", PasswordHash = "fixture-hash", Role = RoleEnum.CanBoXa };
    var chairmanNoMenu = new User { UserName = "chairman-no-menu", PasswordHash = "fixture-hash", Role = RoleEnum.ChuTichXa };
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
        await db.Database.EnsureCreatedAsync(); db.Users.AddRange(admin, staff, chairman, chairmanNoMenu);
        var birthModule = new Service.TanAn.Domain.Entities.Module { TenModule = "Khai sinh", LienKet = "/bien-dong/khai-sinh" };
        var chairmanRole = new Role { RoleName = "Quyền khai sinh", RoleCode = "ctx" };
        var noMenuRole = new Role { RoleName = "CTX thiếu menu", RoleCode = "CTX" };
        db.Modules.Add(birthModule); db.Roles.AddRange(chairmanRole, noMenuRole);
        db.UserRoles.Add(new() { UserId = chairman.Id, RoleId = chairmanRole.Id });
        db.UserRoles.Add(new() { UserId = chairmanNoMenu.Id, RoleId = noMenuRole.Id });
        db.RoleModules.Add(new() { RoleId = chairmanRole.Id, ModuleId = birthModule.Id });
        await db.SaveChangesAsync();
    }
    LoginSession Session(User user) => new(Guid.NewGuid().ToString("N"), user.Id, user.UserName,
        DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(10), null, null,
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.PasswordHash))));
    var adminSession = Session(admin); var staffSession = Session(staff);
    await sessions.CreateAsync(adminSession); await sessions.CreateAsync(staffSession);
    var chairmanSession = Session(chairman); var chairmanNoMenuSession = Session(chairmanNoMenu);
    await sessions.CreateAsync(chairmanSession); await sessions.CreateAsync(chairmanNoMenuSession);
    await app.StartAsync();
    try
    {
        var baseUrl = app.Urls.Single() + "/api/v1/";
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        void Check(bool valid, string label) { if (!valid) throw new Exception(label); Console.WriteLine("PASS: " + label); }
        var filterVillageA = new ApThon { Ma = "API-A", Ten = "Thôn API A" };
        var filterVillageB = new ApThon { Ma = "API-B", Ten = "Thôn API B" };
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            var house = new HoGiaDinh { MaSoHo = "DATE-API", TenChuHo = "Date fixture", DiaChi = "Test", DiaBan = filterVillageA, ApThon = filterVillageA.Ten };
            var person = new NhanKhau { HoTen = "Date fixture", CCCD = "777777777777", HoGiaDinh = house };
            db.NhanKhaus.Add(person);
            foreach (var date in new[] { new DateTime(2024, 1, 10), new DateTime(2024, 1, 11, 23, 59, 59), new DateTime(2024, 1, 12) })
                db.BienDongDanCus.Add(new() { NhanKhau = person, LoaiBienDong = LoaiBienDongEnum.TamTru, NgayPhatSinh = date, LyDo = "Date API fixture" });
            db.BienDongDanCus.Add(new() { NhanKhau = person, LoaiBienDong = LoaiBienDongEnum.KhaiSinh, NgayPhatSinh = new(2024, 1, 11), LyDo = "Date API fixture" });
            await db.SaveChangesAsync();
        }
        var dateQuery = new BienDongQuery { Keyword = "777777777777", LoaiBienDong = (int)LoaiBienDongEnum.TamTru, TuNgay = new(2024, 1, 10), DenNgay = new(2024, 1, 11) };
        using (var response = await http.GetAsync($"BienDong?keyword={dateQuery.Keyword}&loaiBienDong={dateQuery.LoaiBienDong}&tuNgay=2024-01-10&denNgay=2024-01-11&pageSize=1"))
        {
            var result = await response.Content.ReadFromJsonAsync<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<BienDongDto>>>();
            Check(response.IsSuccessStatusCode && result?.Data?.TotalCount == 2 && result.Data.Items.Count == 1, "population API binds type/date filters before pagination");
        }
        int ExcelRows(byte[] bytes)
        {
            using var zip = new System.IO.Compression.ZipArchive(new MemoryStream(bytes));
            using var stream = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            return System.Xml.Linq.XDocument.Load(stream).Descendants(System.Xml.Linq.XName.Get("row", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")).Count();
        }
        using (var response = await http.PostAsJsonAsync("BienDong/xuat-excel", dateQuery))
            Check(response.IsSuccessStatusCode && ExcelRows(await response.Content.ReadAsByteArrayAsync()) == 3, "population API Excel matches combined filters and includes end-of-day records");
        using (var scope = app.Services.CreateScope())
        {
            var localCalls = new CallServiceRegistry(scope.ServiceProvider, new ExcelExportService(), null!);
            var file = await localCalls.PostForFile(new() { Endpoint = "/BienDong/xuat-excel" }, dateQuery);
            Check(file.Success && file.Data != null && ExcelRows(file.Data) == 3, "UI export registry forwards type and date filters");
        }
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            var personB = new NhanKhau
            {
                HoTen = "Date village B", CCCD = "666666666666",
                HoGiaDinh = new() { MaSoHo = "DATE-API-B", DiaBan = filterVillageB, ApThon = filterVillageB.Ten }
            };
            db.BienDongDanCus.Add(new() { NhanKhau = personB, LoaiBienDong = LoaiBienDongEnum.TamTru, NgayPhatSinh = new(2024, 1, 11), LyDo = "Date API fixture" });
            await db.SaveChangesAsync();
        }
        dateQuery.Keyword = "Date API fixture";
        dateQuery.ApThonId = filterVillageA.Id;
        using (var response = await http.GetAsync($"BienDong?keyword={Uri.EscapeDataString(dateQuery.Keyword)}&loaiBienDong={dateQuery.LoaiBienDong}&tuNgay=2024-01-10&denNgay=2024-01-11&apThonId={filterVillageA.Id}&pageSize=1"))
        {
            var result = await response.Content.ReadFromJsonAsync<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<BienDongDto>>>();
            Check(response.IsSuccessStatusCode && result?.Data?.TotalCount == 2 && result.Data.Items.Count == 1,
                "population API combines village, type, keyword and dates before pagination");
        }
        using (var response = await http.PostAsJsonAsync("BienDong/xuat-excel", dateQuery))
            Check(response.IsSuccessStatusCode && ExcelRows(await response.Content.ReadAsByteArrayAsync()) == 3, "population API Excel excludes the other village");
        using (var scope = app.Services.CreateScope())
        {
            var localCalls = new CallServiceRegistry(scope.ServiceProvider, new ExcelExportService(), null!);
            var file = await localCalls.PostForFile(new() { Endpoint = "/BienDong/xuat-excel" }, dateQuery);
            Check(file.Success && file.Data != null && ExcelRows(file.Data) == 3, "UI export registry forwards village alongside type/date/search filters");
        }
        dateQuery.ApThonId = filterVillageB.Id;
        using (var response = await http.PostAsJsonAsync("BienDong/xuat-excel", dateQuery))
            Check(response.IsSuccessStatusCode && ExcelRows(await response.Content.ReadAsByteArrayAsync()) == 2, "switching village changes Excel to that village's records");
        using (var response = await http.GetAsync("BienDong?tuNgay=2024-01-12&denNgay=2024-01-10"))
            Check(response.StatusCode == HttpStatusCode.BadRequest, "population API rejects inverted date ranges");
        using (var birthHttp = new HttpClient { BaseAddress = new Uri(baseUrl) })
        {
            using (var response = await birthHttp.GetAsync("BienDong/khai-sinh/ho-gia-dinh?maSoHo=DATE-API"))
                Check(response.StatusCode == HttpStatusCode.Unauthorized, "birth lookup requires an authenticated session");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", staffSession.Id);
            using (var response = await birthHttp.GetAsync("BienDong/khai-sinh/ho-gia-dinh?maSoHo=DATE-API"))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "birth lookup rejects staff without the birth menu grant");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", adminSession.Id);
            var house = await birthHttp.GetFromJsonAsync<Service.Shared.Commons.Models.ApiResult<HoGiaDinhDto>>("BienDong/khai-sinh/ho-gia-dinh?maSoHo=DATE-API");
            Check(house?.Success == true && house.Data?.ThanhVien?.Count == 1, "birth API household lookup returns the selected household's members");
            var declaration = new KhaiSinhForm
            {
                HoGiaDinhId = house!.Data!.Id, HoTen = "Trẻ qua API", NgaySinh = new(2024, 4, 10),
                NoiSinh = "Bệnh viện API", QueQuan = "Tân An", ThuongTru = "Test", QuanHeVoiChuHo = "Con",
                NguoiYeuCauId = house.Data.ThanhVien!.Single().Id, HoTenNguoiYeuCau = "Date fixture", SoGiayTo = "777777777777",
                NoiCuTruNguoiYeuCau = "Test", QuanHeVoiTre = "Cha"
            };
            Guid draftId;
            var draftRequest = new KhaiSinhDraftSaveForm { HoSo = declaration };
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh/ho-so", draftRequest))
            {
                var result = await response.Content.ReadFromJsonAsync<Service.Shared.Commons.Models.ApiResult<KhaiSinhDraftDto>>();
                Check(response.IsSuccessStatusCode && result?.Data?.HoSo?.HoTen == declaration.HoTen && result.Data.PhienBan == 1,
                    "birth API stores declaration as draft");
                draftId = result!.Data!.Id;
            }
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh/ho-so", draftRequest))
            {
                var result = await response.Content.ReadFromJsonAsync<Service.Shared.Commons.Models.ApiResult<KhaiSinhDraftDto>>();
                Check(response.IsSuccessStatusCode && result?.Data?.Id == draftId && result.Data.PhienBan == 1, "retrying draft over HTTP returns the same draft");
            }
            var reopened = await birthHttp.GetFromJsonAsync<Service.Shared.Commons.Models.ApiResult<KhaiSinhDraftDto>>($"BienDong/khai-sinh/ho-so/{draftId}");
            Check(reopened?.Data?.HoSo?.HoTen == declaration.HoTen, "draft detail API reopens saved declaration");
            var approvalPath = $"BienDong/khai-sinh/ho-so/{draftId}/duyet";
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 1 }))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "even Admin cannot approve birth declarations");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", staffSession.Id);
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 1 }))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "staff cannot bypass the disabled approval button through API");
            birthHttp.DefaultRequestHeaders.Authorization = null;
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 1 }))
                Check(response.StatusCode == HttpStatusCode.Unauthorized, "anonymous callers cannot approve births");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", chairmanNoMenuSession.Id);
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 1 }))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "chairman still needs the birth menu grant");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", adminSession.Id);
            declaration.HoTen = "Trẻ qua API đã sửa"; draftRequest.PhienBan = 1;
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh/ho-so", draftRequest))
                Check(response.IsSuccessStatusCode, "officer can edit declaration before approval");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", chairmanSession.Id);
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
                var approvalRole = await db.Roles.SingleAsync(x => x.RoleCode == "ctx");
                approvalRole.ModerationStatus = ModerationStatus.Pending; await db.SaveChangesAsync();
            }
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 2 }))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "unapproved ctx role cannot approve using existing session");
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
                var approvalRole = await db.Roles.SingleAsync(x => x.RoleCode == "ctx");
                approvalRole.ModerationStatus = ModerationStatus.Approved; await db.SaveChangesAsync();
            }
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 1 }))
                Check(response.StatusCode == HttpStatusCode.Conflict, "chairman cannot approve a stale declaration version");
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 2 }))
            {
                var result = await response.Content.ReadFromJsonAsync<Service.Shared.Commons.Models.ApiResult<KhaiSinhDraftDto>>();
                Check(response.IsSuccessStatusCode && result?.Data?.ModerationStatus == ModerationStatus.Approved && result.Data.NguoiDuyet == chairman.UserName
                    && result.Data.NgayDuyet.HasValue && result.Data.PhienBan == 3, "ctx assignment approves with author and timestamp without chairman account enum");
            }
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 2 }))
                Check(response.IsSuccessStatusCode, "approval retry is idempotent");
            using (var response = await birthHttp.GetAsync("BienDong/khai-sinh/ho-so?choDuyet=true"))
            {
                var result = await response.Content.ReadFromJsonAsync<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<KhaiSinhDraftDto>>>();
                Check(response.IsSuccessStatusCode && result?.Data?.TotalCount == 0, "pending-only API excludes approved declaration");
            }
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
                Check(await db.AuditLogs.CountAsync(x => x.EntityId == draftId.ToString() && x.Action == "Duyệt hồ sơ khai sinh") == 1,
                    "retry does not duplicate approval audit");
                db.UserRoles.Remove(await db.UserRoles.SingleAsync(x => x.UserId == chairman.Id));
                await db.SaveChangesAsync();
            }
            using (var response = await birthHttp.PostAsJsonAsync(approvalPath, new KhaiSinhApproveForm { PhienBan = 3 }))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "revoked ctx assignment is rejected immediately even with existing session");
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", adminSession.Id);
            draftRequest.PhienBan = 3; declaration.HoTen = "Không được sửa hồ sơ đã duyệt";
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh/ho-so", draftRequest))
                Check(response.StatusCode == HttpStatusCode.BadRequest, "approved declaration cannot be edited even by Admin");
            declaration.HoTen = "Trẻ qua API đã sửa";
            var incomplete = new KhaiSinhDraftSaveForm { HoSo = new() { HoTen = "Nháp API thiếu thông tin", NgayDangKy = null } };
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh/ho-so", incomplete))
                Check(response.IsSuccessStatusCode, "API accepts incomplete manual draft without household despite final declaration Required attributes");
            incomplete.HoSo.SoDinhDanh = "123";
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh/ho-so", incomplete))
                Check(response.StatusCode == HttpStatusCode.BadRequest, "draft API validates entered identity format");
            using (var response = await birthHttp.PostAsJsonAsync("BienDong/khai-sinh", declaration))
                Check(response.StatusCode == HttpStatusCode.Conflict, "legacy birth API cannot bypass draft flow to create population");
            var draftList = await birthHttp.GetFromJsonAsync<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<KhaiSinhDraftDto>>>("BienDong/khai-sinh/ho-so?pageSize=1");
            Check(draftList?.Data?.TotalCount == 2 && draftList.Data.Items.Count == 1, "draft API paginates on server");
            var pendingList = await birthHttp.GetFromJsonAsync<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<KhaiSinhDraftDto>>>("BienDong/khai-sinh/ho-so?choDuyet=true&pageSize=1");
            Check(pendingList?.Data?.TotalCount == 1 && pendingList.Data.Items.Single().HoTenTre == incomplete.HoSo.HoTen,
                "pending filter is applied before HTTP pagination and returns remaining draft");
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
                var registered = await db.BienDongDanCus.SingleAsync(x => x.Id == draftId);
                Check(await db.NhanKhaus.CountAsync(x => x.HoTen == declaration.HoTen) == 1
                    && await db.ThanhVienHos.CountAsync(x => x.NhanKhauId == registered.NhanKhauId && x.HoGiaDinhId == house!.Data!.Id) == 1
                    && await db.NhanKhaus.CountAsync(x => x.HoTen == incomplete.HoSo.HoTen) == 0,
                    "HTTP approval creates exactly one child and membership, while unapproved drafts create none");
            }
            birthHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", staffSession.Id);
            using (var response = await birthHttp.GetAsync($"BienDong/khai-sinh/ho-so/{draftId}"))
                Check(response.StatusCode == HttpStatusCode.Forbidden, "draft API rejects staff without menu grant");
            birthHttp.DefaultRequestHeaders.Authorization = null;
            using (var response = await birthHttp.GetAsync("BienDong/khai-sinh/ho-so"))
                Check(response.StatusCode == HttpStatusCode.Unauthorized, "draft API rejects anonymous reads");
        }
        // Remove only the approval test grants so the original administration fixtures stay empty.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            var fixtureRoles = await db.Roles.Where(x => x.RoleCode == "ctx" || x.RoleCode == "CTX").ToListAsync();
            var fixtureRoleIds = fixtureRoles.Select(x => x.Id).ToArray();
            db.UserRoles.RemoveRange(await db.UserRoles.Where(x => Enumerable.Contains(fixtureRoleIds, x.RoleId)).ToListAsync());
            db.RoleModules.RemoveRange(await db.RoleModules.Where(x => Enumerable.Contains(fixtureRoleIds, x.RoleId)).ToListAsync());
            db.Roles.RemoveRange(fixtureRoles);
            db.Modules.Remove(await db.Modules.SingleAsync(x => x.LienKet == "/bien-dong/khai-sinh"));
            await db.SaveChangesAsync();
        }
        using (var response = await http.GetAsync("administration/users"))
            Check(response.StatusCode == HttpStatusCode.Unauthorized, "API rejects anonymous callers");
        using (var response = await http.GetAsync("system-configuration/general"))
            Check(response.StatusCode == HttpStatusCode.Unauthorized, "configuration rejects anonymous callers");
        using (var response = await http.GetAsync("system-configuration/public"))
            Check(response.StatusCode == HttpStatusCode.OK, "public display configuration is available anonymously");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", staffSession.Id);
        using (var response = await http.GetAsync("administration/users"))
            Check(response.StatusCode == HttpStatusCode.Forbidden, "API rejects non-admin sessions");
        using (var response = await http.GetAsync("system-configuration/general"))
            Check(response.StatusCode == HttpStatusCode.Forbidden, "configuration rejects non-admin sessions");

        var authentication = new TestAuthentication(adminSession.Id);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ServiceEndpoints:ServiceTanAn"] = baseUrl }).Build();
        var transport = new ApiServiceTransport(new TestClientFactory(), config, authentication,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ApiServiceTransport>.Instance);
        ICallServiceRegistry calls = new CallServiceRegistry(app.Services, new ExcelExportService(), transport);
        ApiRequestModel Request(string path) => new() { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/administration/" + path };
        var users = (await calls.Get<List<AdminRecord>>(Request("users"))).RequireData();
        Check(users.Count == 4, "registry reads users over HTTP");
        foreach (var path in new[] { "roles", "groups", "menus", "modules", "parameters" })
            Check((await calls.Get<List<AdminRecord>>(Request(path))).RequireData().Count == 0, "registry reads " + path + " without creating defaults");
        (await calls.Post(Request("menus"), new AdminRecord { Name = "Menu qua HTTP", Path = "/ho-khau", Active = true })).EnsureSuccess();
        var menu = (await calls.Get<List<AdminRecord>>(Request("menus"))).RequireData().Single();
        (await calls.Post(Request("roles"), new AdminRecord { Code = "API_ROLE", Name = "Vai trò qua HTTP", AssignedIds = [menu.Id], Active = true })).EnsureSuccess();
        var role = (await calls.Get<List<AdminRecord>>(Request("roles"))).RequireData().Single();
        Check(role.AssignedIds.SequenceEqual(new[] { menu.Id }), "role menu assignments persist through API");
        var staffForm = users.Single(x => x.Id == staff.Id);
        staffForm.AssignedIds = [role.Id];
        (await calls.Post(Request("users"), staffForm)).EnsureSuccess();
        Check((await calls.Get<List<AdminRecord>>(Request("users"))).RequireData().Single(x => x.Id == staff.Id).AssignedIds.Contains(role.Id), "user role assignments persist through API");
        menu.Name = "Tên được sửa qua HTTP";
        (await calls.Post(Request("menus"), menu)).EnsureSuccess();
        Check((await calls.Get<List<AdminRecord>>(Request("menus"))).RequireData().Single().Name == menu.Name, "editing a menu preserves its identity");
        var invalid = await calls.Post(Request("menus"), new AdminRecord { Name = "URL lỗi", Path = "//external.example" });
        Check(!invalid.Success && invalid.Status == Service.Shared.Commons.Interfaces.StatusCode.BadRequest && !string.IsNullOrEmpty(invalid.Message), "validation failures propagate to the page");
        Check(!(await calls.Delete(Request("menus/" + menu.Id))).Success, "API protects assigned menu deletion");
        (await calls.Post(Request("menus"), new AdminRecord { Name = "Menu tạm", Path = "/temporary" })).EnsureSuccess();
        var temporary = (await calls.Get<List<AdminRecord>>(Request("menus"))).RequireData().Single(x => x.Path == "/temporary");
        (await calls.Delete(Request("menus/" + temporary.Id))).EnsureSuccess();
        Check((await calls.Get<List<AdminRecord>>(Request("menus"))).RequireData().Count == 1, "registry delete uses HTTP and persists");
        Check((await calls.Get<List<AdministrationVillageDto>>(Request("villages"))).RequireData().Count == 0, "village options load through API");
        ApiRequestModel Configuration(string action) => new() { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/system-configuration/" + action };
        var defaults = (await calls.Get<List<SystemConfigurationField>>(Configuration("general"))).RequireData();
        Check(defaults.Count == 16 && defaults.All(x => x.IsDefault), "typed defaults load without inserting database rows");
        await using (var renderer = new Microsoft.AspNetCore.Components.Web.HtmlRenderer(app.Services, app.Services.GetRequiredService<ILoggerFactory>()))
        {
            var html = await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var component = await renderer.RenderComponentAsync<Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.ThamSoHeThong.ParameterValueEditor>(
                    Microsoft.AspNetCore.Components.ParameterView.FromDictionary(new Dictionary<string, object?>
                    { ["Definition"] = SystemParameterCatalog.Find("HeaderContent"), ["Value"] = "<script>alert('test')</script>" }));
                return component.ToHtmlString();
            });
            Check(html.Contains("&lt;script&gt;") && !html.Contains("<script>"), "parameter text editor initializes value and escapes markup");
            foreach (var status in Enum.GetValues<ModerationStatus>().Append((ModerationStatus)99))
            {
                var statusHtml = await renderer.Dispatcher.InvokeAsync(async () =>
                {
                    var component = await renderer.RenderComponentAsync<Service.UI.CMS.Blazor.Components.Shared.ModerationView>(
                        Microsoft.AspNetCore.Components.ParameterView.FromDictionary(new Dictionary<string, object?> { ["Status"] = status }));
                    return WebUtility.HtmlDecode(component.ToHtmlString());
                });
                var approved = status == ModerationStatus.Approved;
                Check(statusHtml.Contains(approved ? "Đã duyệt" : "Chưa duyệt")
                    && statusHtml.Contains(approved ? "moderation approved" : "moderation pending"),
                    $"legacy status {status} renders as one of two approval states");
            }
        }
        var validConfiguration = new List<SystemConfigurationUpdate>
        {
            new() { Code = " appname ", Value = "Tên ứng dụng kiểm thử", ExpectedValue = defaults.Single(x => x.Code == "AppName").Value },
            new() { Code = "PasswordMinLength", Value = "12" },
            new() { Code = "HeaderEnabled", Value = "TRUE" },
            new() { Code = "HeaderContent", Value = "<script>alert('test')</script>" }
        };
        (await calls.Put(Configuration("general"), validConfiguration)).EnsureSuccess();
        var publicConfig = (await calls.Get<PublicSystemConfigurationDto>(Configuration("public"))).RequireData();
        Check(publicConfig.AppName == "Tên ứng dụng kiểm thử" && publicConfig.HeaderEnabled, "configuration values are effective immediately");
        var policy = (await calls.Get<PasswordPolicyDto>(Configuration("password-policy"))).RequireData();
        Check(policy.MinLength == 12 && policy.RequireUppercase, "password policy uses stored validated values");
        Check(!(await calls.Put(Configuration("general"), new List<SystemConfigurationUpdate>
        { new() { Code = "AppName", Value = "Must not be saved" }, new() { Code = "MinuteExpireToken", Value = "0" } })).Success,
            "mixed batch with out-of-range integer rejected");
        Check((await calls.Get<PublicSystemConfigurationDto>(Configuration("public"))).RequireData().AppName == publicConfig.AppName,
            "invalid batch does not partially save valid fields");
        Check(!(await calls.Put(Configuration("general"), new List<SystemConfigurationUpdate>
        { new() { Code = "HeaderEnabled", Value = "yes" } })).Success, "invalid boolean rejected");
        Check(!(await calls.Put(Configuration("general"), new List<SystemConfigurationUpdate>
        { new() { Code = "AppName", Value = "Conflict", ExpectedValue = "stale" } })).Success, "stale form rejected");
        Check(!(await calls.Put(Configuration("general"), new List<SystemConfigurationUpdate>
        { new() { Code = "Hotline", Value = "1" }, new() { Code = "hotline", Value = "2" } })).Success, "duplicate batch codes rejected case-insensitively");
        Check(!(await calls.Put(Configuration("general"), new List<SystemConfigurationUpdate>
        { new() { Code = "PrivateApiKey", Value = "fixture-secret" } })).Success, "general configuration rejects undeclared keys");
        (await calls.Post(Request("parameters"), new AdminRecord { Code = "PrivateApiKey", Path = "fixture-secret" })).EnsureSuccess();
        Check(!(await calls.Post(Request("parameters"), new AdminRecord { Code = " privateapikey ", Path = "duplicate" })).Success,
            "parameter catalog rejects duplicate trimmed codes");
        using (var publicResponse = await http.GetAsync("system-configuration/public"))
            Check(!(await publicResponse.Content.ReadAsStringAsync()).Contains("fixture-secret"), "public configuration does not disclose custom parameters");
        var parameters = (await calls.Get<List<AdminRecord>>(Request("parameters"))).RequireData();
        var appName = parameters.Single(x => x.Code == "AppName");
        appName.Code = "RenamedAppName";
        Check(!(await calls.Post(Request("parameters"), appName)).Success, "built-in parameter codes cannot be renamed");
        appName.Code = "AppName"; appName.Active = false;
        (await calls.Post(Request("parameters"), appName)).EnsureSuccess();
        Check((await calls.Get<PublicSystemConfigurationDto>(Configuration("public"))).RequireData().AppName == SystemParameterCatalog.Find("AppName")!.DefaultValue,
            "unapproved setting falls back immediately");
        Check((await calls.Get<List<AdminRecord>>(Request("parameters"))).RequireData().Single(x => x.Id == appName.Id).ModerationStatus == ModerationStatus.Pending,
            "canceling approval saves the pending status");
        using (var scope = app.Services.CreateScope())
        {
            var configuration = scope.ServiceProvider.GetRequiredService<SystemConfigurationService>();
            await configuration.ChangeStatusAsync(appName.Id, ModerationStatus.Rejected,
                new Service.Shared.Commons.Models.CurrentUserDto { UserId = admin.Id, UserName = admin.UserName, Role = "Admin", IsAuthenticated = true });
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            Check((await db.SystemParameters.FindAsync(appName.Id))!.ModerationStatus == ModerationStatus.Pending,
                "legacy reject action saves pending status");
        }
        appName.Active = true;
        (await calls.Post(Request("parameters"), appName)).EnsureSuccess();
        Check((await calls.Get<PublicSystemConfigurationDto>(Configuration("public"))).RequireData().AppName == publicConfig.AppName,
            "approving a pending parameter restores its configured value");
        appName.Active = false;
        (await calls.Post(Request("parameters"), appName)).EnsureSuccess();
        Check(!(await calls.Delete(Request("parameters/" + appName.Id))).Success, "built-in parameters cannot be deleted");
        (await calls.Post(Configuration("sync"), new { })).EnsureSuccess();
        (await calls.Post(Configuration("sync"), new { })).EnsureSuccess();
        parameters = (await calls.Get<List<AdminRecord>>(Request("parameters"))).RequireData();
        Check(parameters.Count == 17 && parameters.Single(x => x.Code == "PasswordMinLength").Path == "12",
            "explicit sync is idempotent and preserves stored values");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            (await db.SystemParameters.SingleAsync(x => x.Code == "HeaderEnabled")).Value = "TRUE";
            await db.SaveChangesAsync();
        }
        Check(!(await calls.Get<List<SystemConfigurationField>>(Configuration("general"))).RequireData().Single(x => x.Code == "HeaderEnabled").IsDefault,
            "valid legacy boolean is shown as stored configuration after normalization");
        Guid duplicateId;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            var duplicate = new SystemParameter { Code = "headerenabled", Value = "true" };
            duplicateId = duplicate.Id; db.SystemParameters.Add(duplicate); await db.SaveChangesAsync();
        }
        Check(!(await calls.Get<PublicSystemConfigurationDto>(Configuration("public"))).RequireData().HeaderEnabled,
            "ambiguous duplicate legacy configuration uses validated default");
        Check(!(await calls.Put(Configuration("general"), new List<SystemConfigurationUpdate>
        { new() { Code = "HeaderEnabled", Value = "true" } })).Success, "batch refuses ambiguous duplicate legacy code");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            db.SystemParameters.Remove((await db.SystemParameters.FindAsync(duplicateId))!); await db.SaveChangesAsync();
        }
        (await calls.Delete(Request("parameters/" + parameters.Single(x => x.Code == "PrivateApiKey").Id))).EnsureSuccess();
        Check(!(await calls.Post(Request("users"), new AdminRecord { Code = "weak-user", Name = "Weak user", Password = "password" })).Success,
            "account creation enforces configured password policy");
        (await calls.Post(Request("users"), new AdminRecord { Code = "strong-user", Name = "Strong user", Password = "Valid-Test-12345" })).EnsureSuccess();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            Check(await db.AuditLogs.AllAsync(x => x.Action == "Duyệt hồ sơ khai sinh" || x.Action == "Tạo nhân khẩu" || x.Action == "Đăng ký khai sinh"
                ? x.Username == chairman.UserName : x.Username == admin.UserName), "audit actor comes from authenticated session");
            Check(await db.AuditLogs.AnyAsync(x => x.EntityName == "Parameters" && x.NewValues!.Contains("Tên ứng dụng kiểm thử"))
                || await db.AuditLogs.AnyAsync(x => x.EntityName == "Parameters" && x.NewValues!.Contains("AppName")), "parameter edits are audited with before/after snapshots");
            Check(!await db.AuditLogs.AnyAsync(x => x.NewValues!.Contains("fixture-secret") || x.OldValues!.Contains("fixture-secret")),
                "custom parameter values are redacted from audit logs");
        }
        User passwordUser;
        using (var scope = app.Services.CreateScope())
            passwordUser = await scope.ServiceProvider.GetRequiredService<TanAnDbContext>().Users.SingleAsync(x => x.UserName == "strong-user");
        var passwordSession = Session(passwordUser);
        await sessions.CreateAsync(passwordSession);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TanAnSession", passwordSession.Id);
        using (var response = await http.PutAsJsonAsync("session-account/password", new ChangeOwnPasswordForm { CurrentPassword = "wrong", NewPassword = "New-Test-Password-123" }))
            Check(response.StatusCode == HttpStatusCode.BadRequest, "password change requires current password");
        using (var response = await http.PutAsJsonAsync("session-account/password", new ChangeOwnPasswordForm { CurrentPassword = "Valid-Test-12345", NewPassword = "weak" }))
            Check(response.StatusCode == HttpStatusCode.BadRequest, "password change enforces current system policy");
        using (var response = await http.PutAsJsonAsync("session-account/password", new ChangeOwnPasswordForm { CurrentPassword = "Valid-Test-12345", NewPassword = "New-Test-Password-123" }))
            Check(response.StatusCode == HttpStatusCode.OK, "non-admin can change only their own password through session API");
        using (var response = await http.PutAsJsonAsync("session-account/password", new ChangeOwnPasswordForm { CurrentPassword = "New-Test-Password-123", NewPassword = "Another-Password-123" }))
            Check(response.StatusCode == HttpStatusCode.Unauthorized, "password change invalidates old login session");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
            var changedUser = await db.Users.FindAsync(passwordUser.Id);
            Check(Service.Shared.Commons.Helpers.PasswordHashing.Verify("New-Test-Password-123", changedUser!.PasswordHash), "changed password stored as verified hash");
            Check(!await db.AuditLogs.AnyAsync(x => x.NewValues!.Contains("New-Test-Password-123")), "password never appears in audit snapshots");
        }
        await sessions.RevokeAsync(adminSession.Id);
        Check((await calls.Get<List<AdminRecord>>(Request("users"))).Status == Service.Shared.Commons.Interfaces.StatusCode.Unauthorized, "revoked session is denied by API");
        Console.WriteLine("All API checks passed; only local HTTP, in-memory sessions and SQLite were used.");
    }
    finally { await app.StopAsync(); }
}
catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }

sealed class TestClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new(new HttpClientHandler { AllowAutoRedirect = false });
}
sealed class TestKeyRepository : Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository
{
    private readonly List<System.Xml.Linq.XElement> values = new();
    public IReadOnlyCollection<System.Xml.Linq.XElement> GetAllElements() => values.ToList();
    public void StoreElement(System.Xml.Linq.XElement element, string friendlyName) => values.Add(new(element));
}
sealed class TestAuthentication(string sessionId) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(
        new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(AccountService.SessionClaim, sessionId) }, "test"))));
}
sealed class TestSessions : ILoginSessionStore
{
    private readonly Dictionary<string, LoginSession> items = new();
    public Task CreateAsync(LoginSession session) { items[session.Id] = session; return Task.CompletedTask; }
    public Task<LoginSession?> FindAsync(string id) => Task.FromResult(items.GetValueOrDefault(id));
    public Task<IReadOnlyList<LoginSession>> ListAsync() => Task.FromResult<IReadOnlyList<LoginSession>>(items.Values.ToList());
    public Task RevokeAsync(string id) { items.Remove(id); return Task.CompletedTask; }
}
