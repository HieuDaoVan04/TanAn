using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Infrastructure.Persistence;

try
{
    // Cơ sở dữ liệu trong RAM; không đọc/ghi database của ứng dụng.
    using var connection = new SqliteConnection("Data Source=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<TanAnDbContext>().UseSqlite(connection).Options;
    using var db = new TanAnDbContext(options);
    db.Database.EnsureCreated();
    var ho = new HoGiaDinh { MaSoHo = "TEST-001", TenChuHo = "Test", DiaChi = "Test" };
    var nguoi = new NhanKhau { HoTen = "Test", CCCD = "012345678901", HoGiaDinh = ho };
    db.Add(nguoi);
    db.SaveChanges();
    var hoId = ho.Id;
    var nguoiId = nguoi.Id;
    db.ChangeTracker.Clear();
    
    void Reject(string name, Action<TanAnDbContext> action)
    {
        using var test = new TanAnDbContext(options);
        action(test);
        try { test.SaveChanges(); }
        catch (DbUpdateException) { Console.WriteLine($"PASS: {name}"); return; }
        throw new Exception($"Expected database rejection: {name}");
    }
    
    Reject("duplicate CCCD", x => x.Add(new NhanKhau { HoTen = "Duplicate", CCCD = "012345678901", MaHoGiaDinh = hoId }));
    db.AddRange(new NhanKhau { HoTen = "Child 1", MaHoGiaDinh = hoId }, new NhanKhau { HoTen = "Child 2", MaHoGiaDinh = hoId });
    db.SaveChanges();
    Console.WriteLine("PASS: multiple residents without CCCD");
    db.ChangeTracker.Clear();
    Reject("household deletion preserves residents", x => x.Remove(x.HoGiaDinhs.Single()));
    Reject("invalid history dates", x => x.Add(new ThanhVienHo { HoGiaDinhId = hoId, NhanKhauId = nguoiId, TuNgay = new(2026, 2, 1), DenNgay = new(2026, 1, 1) }));
    db.ThanhVienHos.Add(new ThanhVienHo { HoGiaDinhId = hoId, NhanKhauId = nguoiId, TuNgay = new(2026, 1, 1), LaChuHo = true });
    db.SaveChanges();
    Reject("one open membership per resident", x => x.Add(new ThanhVienHo { HoGiaDinhId = hoId, NhanKhauId = nguoiId, TuNgay = new(2026, 2, 1) }));
    var childId = db.NhanKhaus.First(x => x.CCCD == "").Id;
    Reject("one current household head", x => x.Add(new ThanhVienHo { HoGiaDinhId = hoId, NhanKhauId = childId, TuNgay = new(2026, 2, 1), LaChuHo = true }));
    Reject("foreign key to existing household", x => x.Add(new PhanLoaiHo { HoGiaDinhId = Guid.NewGuid(), TuNgay = new(2026, 1, 1) }));
    Reject("negative benefit amount", x => x.Add(new DoiTuongAnSinh { NhanKhauId = nguoiId, MucTroCapHangThang = -1 }));
    var request = new YeuCauNguoiDan { MaYeuCau = "HS-001" };
    db.Add(request);
    db.SaveChanges();
    Reject("unique request number", x => x.Add(new YeuCauNguoiDan { MaYeuCau = "HS-001" }));
    db.TepDinhKems.Add(new TepDinhKem { YeuCauId = request.Id, TenTep = "a.pdf", DuongDanLuu = "test/a.pdf", LoaiTep = "application/pdf" });
    db.SaveChanges();
    Reject("request deletion preserves attachments", x => x.Remove(x.YeuCauNguoiDans.Single()));
    
    using var postgres = new TanAnDbContext(new DbContextOptionsBuilder<TanAnDbContext>()
        .UseNpgsql("Host=localhost;Database=model_only;Username=unused;Password=unused").Options);
    var sql = postgres.Database.GenerateCreateScript();
    if (!sql.Contains("CREATE TABLE") || !sql.Contains("ThanhVienHos")) throw new Exception("PostgreSQL schema generation failed");
    Console.WriteLine("PASS: PostgreSQL model and DDL generation (no server connection)");
    Console.WriteLine("All entity model checks passed.");
    
    var cycleA = new Service.TanAn.Domain.Entities.Module();
    var cycleB = new Service.TanAn.Domain.Entities.Module { ModuleChaId = cycleA.Id };
    cycleA.ModuleChaId = cycleB.Id;
    try
    {
        Service.TanAn.Application.Services.Core.MenuTreeBuilder.Build(new[] { cycleA, cycleB });
        throw new Exception("Cycle was not rejected");
    }
    catch (InvalidOperationException) { Console.WriteLine("PASS: menu cycle rejected"); }

    
    var actor = new Service.Shared.Commons.Models.CurrentUserDto { IsAuthenticated = true, Role = "Admin", UserName = "test-admin" };
    var admin = new Service.TanAn.Application.Services.Core.AdministrationService(db, new Microsoft.AspNetCore.Http.HttpContextAccessor());
    async Task RejectAdmin(string label, Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or UnauthorizedAccessException)
        { Console.WriteLine($"PASS: {label}"); return; }
        throw new Exception($"Expected rejection: {label}");
    }
    await RejectAdmin("anonymous administration denied", () => admin.ListAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Modules, new()));
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Modules, new() { Code = "TEST", Name = "Test module" }, actor);
    var testModule = (await admin.ListAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Modules, actor)).Single(x => x.Code == "TEST");
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Menus, new() { Name = "Test menu", Path = "/test-menu", ModuleId = testModule.Id }, actor);
    var testMenu = (await admin.ListAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Menus, actor)).Single(x => x.Path == "/test-menu");
    testMenu.ParentId = testMenu.Id;
    await RejectAdmin("cyclic menu rejected", () => admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Menus, testMenu, actor));
    testMenu.ParentId = null;
    await RejectAdmin("module with menus cannot be deleted", () => admin.DeleteAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Modules, testModule.Id, actor));
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Roles, new() { Code = "TEST_ROLE", Name = "Test role", AssignedIds = new() { testMenu.Id } }, actor);
    var testRole = (await admin.ListAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Roles, actor)).Single(x => x.Code == "TEST_ROLE");
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Users, new() { Code = "testuser", Name = "Test User", Password = "Test-pass-123", AssignedIds = new() { testRole.Id } }, actor);
    var stored = db.Users.Single(x => x.UserName == "testuser");
    if (!Service.Shared.Commons.Helpers.PasswordHashing.Verify("Test-pass-123", stored.PasswordHash)
        || Service.Shared.Commons.Helpers.PasswordHashing.Verify("wrong", stored.PasswordHash)) throw new Exception("Password hash verification failed");
    if (!db.UserRoles.Any(x => x.UserId == stored.Id && x.RoleId == testRole.Id)) throw new Exception("Role assignment not saved");
    await RejectAdmin("assigned role cannot be deleted", () => admin.DeleteAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Roles, testRole.Id, actor));
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Groups, new() { Code = "UNIT", Name = "Test unit" }, actor);
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, new() { Code = "SETTING", Path = "value" }, actor);
    foreach (var catalog in Enum.GetValues<Service.Shared.Contracts.DTOs.AdminCatalog>())
        if ((await admin.ListAsync(catalog, actor)).Count == 0) throw new Exception($"Empty catalog {catalog}");
    testMenu.Order = 99;
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Menus, testMenu, actor);
    if (db.Modules.Single(x => x.Id == testMenu.Id).ViTri != 99) throw new Exception("Menu edit was not persisted");
    Console.WriteLine("PASS: all six administration catalogs, assignments, password hashing, and menu edit persistence");
    
    // The list must preserve the full moderation state when editing non-status fields.
    var draft = db.SystemParameters.Single(x => x.Code == "SETTING");
    draft.ModerationStatus = Service.Shared.Commons.Model.SQL.ModerationStatus.Draft;
    await db.SaveChangesAsync();
    var draftForm = (await admin.ListAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, actor)).Single(x => x.Code == "SETTING");
    if (draftForm.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Draft) throw new Exception("Draft state was lost in list");
    draftForm.Description = "Updated description";
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, draftForm, actor);
    if (draft.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Draft) throw new Exception("Editing changed draft state");
    draftForm.Active = true;
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, draftForm, actor);
    if (draft.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Approved) throw new Exception("Approval failed");
    Console.WriteLine("PASS: moderation labels retain source state, edits preserve drafts, approval changes state");
    if (Array.IndexOf(args, "--menu-only") >= 0) return;
    
    var auditService = new Service.TanAn.Application.Services.AuditLogService(db);
    var populationService = new Service.TanAn.Application.Services.PopulationService(db, auditService);
    var welfareService = new Service.TanAn.Application.Services.WelfareService(db, auditService);
    var createdHouse = await populationService.CreateHoGiaDinhAsync(new() { MaSoHo = "UI-TEST", TenChuHo = "Test", CCCDChuHo = "999999999999", DiaChi = "Test", ApThon = "Test" }, "ui-test");
    if (!createdHouse.Success || createdHouse.Data == null) throw new Exception($"Create household failed: {createdHouse.Message}");
    var createdPerson = await populationService.CreateNhanKhauAsync(new() { MaHoGiaDinh = createdHouse.Data.Id, HoTen = "Test person", CCCD = "999999999999", NgaySinh = new(1960, 1, 1) }, "ui-test");
    if (!createdPerson.Success || createdPerson.Data == null) throw new Exception("Create resident failed");
    var createdChange = await populationService.CreateBienDongAsync(new() { NhanKhauId = createdPerson.Data.Id, LoaiBienDong = Service.TanAn.Domain.Enums.LoaiBienDongEnum.TamTru, LyDo = "Test", NgayPhatSinh = DateTime.Today }, "ui-test");
    var createdBenefit = await welfareService.CreateDoiTuongAnSinhAsync(new() { NhanKhauId = createdPerson.Data.Id, LoaiDoiTuong = Service.TanAn.Domain.Enums.DoiTuongAnSinhEnum.NguoiCaoTuoi, MucTroCapHangThang = 500000, NgayBatDauHuong = DateTime.Today }, "ui-test");
    if (!createdChange.Success || !createdBenefit.Success || createdBenefit.Data == null) throw new Exception("Create change/benefit failed");
    var payment = await welfareService.AddLichSuTroCapAsync(new() { DoiTuongAnSinhId = createdBenefit.Data.Id, ThangNam = "09/2026", SoTien = 500000 }, "ui-test");
    if (!payment.Success || payment.Data?.NguoiChiTra != "ui-test") throw new Exception("Payment save failed");
    Console.WriteLine("PASS: operational create actions persist household, resident, change, benefit and payout with actor");
}
catch (Exception ex)
{
    // Report a failed check without invoking the Windows unhandled-exception dialog.
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
