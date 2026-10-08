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
    
    // Legacy moderation values remain readable; saving normalizes them to the two approval states.
    var draft = db.SystemParameters.Single(x => x.Code == "SETTING");
    draft.ModerationStatus = Service.Shared.Commons.Model.SQL.ModerationStatus.Draft;
    await db.SaveChangesAsync();
    var draftForm = (await admin.ListAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, actor)).Single(x => x.Code == "SETTING");
    if (draftForm.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Draft) throw new Exception("Draft state was lost in list");
    draftForm.Description = "Updated description";
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, draftForm, actor);
    if (draft.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Pending) throw new Exception("Legacy draft did not become pending on save");
    draftForm.Active = true;
    await admin.SaveAsync(Service.Shared.Contracts.DTOs.AdminCatalog.Parameters, draftForm, actor);
    if (draft.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Approved) throw new Exception("Approval failed");
    Console.WriteLine("PASS: legacy moderation stays readable, edits normalize to pending, approval changes state");
    if (Array.IndexOf(args, "--menu-only") >= 0) return;
    
    var auditService = new Service.TanAn.Application.Services.AuditLogService(db);
    var populationService = new Service.TanAn.Application.Services.PopulationService(db, auditService);
    var welfareService = new Service.TanAn.Application.Services.WelfareService(db, auditService);
    db.ApThons.Add(new() { Ma = "TEST-TH", Ten = "Test", DangHoatDong = true });
    await db.SaveChangesAsync();
    var createdHouse = await populationService.CreateHoGiaDinhAsync(new() { MaSoHo = "UI-TEST", TenChuHo = "Test", CCCDChuHo = "888888888888", NgaySinhChuHo = new(1960, 1, 1), DiaChi = "Test", ApThon = "Test" }, "ui-test");
    if (!createdHouse.Success || createdHouse.Data == null) throw new Exception($"Create household failed: {createdHouse.Message}");
    var createdPerson = await populationService.CreateNhanKhauAsync(new() { MaHoGiaDinh = createdHouse.Data.Id, HoTen = "Test person", CCCD = "999999999999", NgaySinh = new(1960, 1, 1) }, "ui-test");
    if (!createdPerson.Success || createdPerson.Data == null) throw new Exception("Create resident failed");
    var createdChange = await populationService.CreateBienDongAsync(new() { NhanKhauId = createdPerson.Data.Id, LoaiBienDong = Service.TanAn.Domain.Enums.LoaiBienDongEnum.TamTru, LyDo = "Test", NgayPhatSinh = DateTime.Today }, "ui-test");
    foreach (var changeType in Enum.GetValues<Service.TanAn.Domain.Enums.LoaiBienDongEnum>())
    {
        var added = await populationService.CreateBienDongAsync(new() { NhanKhauId = createdPerson.Data.Id, LoaiBienDong = changeType, LyDo = "Menu type test", NoiDenOrDi = "Destination test" }, "ui-test");
        if (changeType == Service.TanAn.Domain.Enums.LoaiBienDongEnum.KhaiSinh)
        {
            if (added.Success) throw new Exception("Generic change API bypasses draft birth flow");
            db.BienDongDanCus.Add(new() { NhanKhauId = createdPerson.Data.Id, LoaiBienDong = changeType, LyDo = "Historical birth fixture", NoiDenOrDi = "Destination test", NgayPhatSinh = DateTime.Today });
            await db.SaveChangesAsync();
        }
        else if (!added.Success) throw new Exception($"Create failed for {changeType}");
        var filtered = await populationService.GetBienDongsAsync(null, 1, 1, (int)changeType);
        var expectedCount = changeType == Service.TanAn.Domain.Enums.LoaiBienDongEnum.TamTru ? 2 : 1;
        if (!filtered.Success || filtered.Data?.TotalCount != expectedCount || filtered.Data.Items.Count != 1 || filtered.Data.Items.Any(x => x.LoaiBienDong != changeType))
            throw new Exception($"Type filter/pagination failed for {changeType}");
    }
    var allChanges = await populationService.GetBienDongsAsync(null, 1, int.MaxValue);
    if (allChanges.Data?.TotalCount != 7 || allChanges.Data.Items.Select(x => x.LoaiBienDong).Distinct().Count() != 6) throw new Exception("Aggregate does not include all six change types");
    var byCccd = await populationService.GetBienDongsAsync("999999999999", 1, 10, (int)Service.TanAn.Domain.Enums.LoaiBienDongEnum.KhaiSinh);
    var byPlace = await populationService.GetBienDongsAsync("Destination test", 1, 10, (int)Service.TanAn.Domain.Enums.LoaiBienDongEnum.KhaiTu);
    if (byCccd.Data?.TotalCount != 1 || byPlace.Data?.TotalCount != 1) throw new Exception("Change search lost type filter");
    if ((await populationService.GetBienDongsAsync(null, 1, 10, 99)).Success) throw new Exception("Unknown change type was accepted");
    Console.WriteLine("PASS: six change categories, aggregate, filtered totals, pagination and CCCD/place search");
    var rangeStart = new DateTime(2024, 1, 10);
    var rangeEnd = new DateTime(2024, 1, 11);
    foreach (var date in new[] { rangeStart.AddDays(-1), rangeStart, rangeEnd.AddDays(1).AddTicks(-1), rangeEnd.AddDays(1) })
        db.BienDongDanCus.Add(new() { NhanKhauId = createdPerson.Data.Id, LoaiBienDong = Service.TanAn.Domain.Enums.LoaiBienDongEnum.TamTru, NgayPhatSinh = date, LyDo = "Date boundary fixture" });
    db.BienDongDanCus.Add(new() { NhanKhauId = createdPerson.Data.Id, LoaiBienDong = Service.TanAn.Domain.Enums.LoaiBienDongEnum.KhaiSinh, NgayPhatSinh = rangeEnd, LyDo = "Date boundary fixture" });
    await db.SaveChangesAsync();
    var dateRange = await populationService.GetBienDongsAsync("Date boundary fixture", 1, 1, null, rangeStart, rangeEnd);
    if (dateRange.Data?.TotalCount != 3 || dateRange.Data.Items.Count != 1) throw new Exception("Date range must filter before counting and pagination");
    var combinedRange = await populationService.GetBienDongsAsync("999999999999", 1, 10, (int)Service.TanAn.Domain.Enums.LoaiBienDongEnum.TamTru, rangeStart.AddHours(15), rangeEnd);
    if (combinedRange.Data?.TotalCount != 2 || !combinedRange.Data.Items.Any(x => x.NgayPhatSinh == rangeStart)
        || !combinedRange.Data.Items.Any(x => x.NgayPhatSinh == rangeEnd.AddDays(1).AddTicks(-1)))
        throw new Exception("Combined type/search/date filter must include both boundary days");
    var fromOnly = await populationService.GetBienDongsAsync("Date boundary fixture", 1, 10, tuNgay: rangeStart);
    var toOnly = await populationService.GetBienDongsAsync("Date boundary fixture", 1, 10, denNgay: rangeEnd);
    var singleDay = await populationService.GetBienDongsAsync("Date boundary fixture", 1, 10, tuNgay: rangeEnd, denNgay: rangeEnd);
    if (fromOnly.Data?.TotalCount != 4 || toOnly.Data?.TotalCount != 4 || singleDay.Data?.TotalCount != 2)
        throw new Exception("Open-ended or same-day date filters failed");
    if ((await populationService.GetBienDongsAsync(null, 1, 10, tuNgay: rangeEnd, denNgay: rangeStart)).Success)
        throw new Exception("Inverted date range was accepted");
    Console.WriteLine("PASS: inclusive date boundaries, single day, open-ended range, combined filters, pagination and invalid range");
    Service.Shared.Contracts.DTOs.KhaiSinhForm BirthForm(string name) => new()
    {
        HoGiaDinhId = createdHouse.Data.Id, HoTen = name, NgaySinh = new(2024, 3, 15),
        NoiSinh = "Bệnh viện thử nghiệm", QueQuan = "Tân An", ThuongTru = "Test",
        QuanHeVoiChuHo = "Cháu", HoTenNguoiYeuCau = "Test person", SoGiayTo = "999999999999",
        NguoiYeuCauId = createdPerson.Data.Id, NoiCuTruNguoiYeuCau = "Test", QuanHeVoiTre = "Cha",
        Cha = new() { NhanKhauId = createdPerson.Data.Id, HoTen = "Test person", NgaySinh = new(1960, 1, 1), NoiCuTru = "Test" }
    };
    var birthHouse = await populationService.GetHoGiaDinhByCodeAsync(" UI-TEST ");
    if (!birthHouse.Success || birthHouse.Data?.ThanhVien?.Count != 2 || birthHouse.Data.ThanhVien.Any(x => x.ThuongTru == null))
        throw new Exception("Household lookup must return its members and autofill fields");
    var newborn = BirthForm("Trẻ thử nghiệm");
    newborn.NgayDangKy = new(2024, 3, 16);
    newborn.QuocTich = "Quốc tịch thử nghiệm";
    var birthResult = await populationService.CreateKhaiSinhAsync(newborn, "birth-officer");
    if (!birthResult.Success || birthResult.Data?.HoSoKhaiSinh?.QuocTich != newborn.QuocTich) throw new Exception($"Birth registration failed: {birthResult.Message}");
    var newPersonId = birthResult.Data.NhanKhauId;
    var newPerson = await db.NhanKhaus.SingleAsync(x => x.Id == newPersonId);
    if (newPerson.MaHoGiaDinh != createdHouse.Data.Id || newPerson.CCCD != "" || newPerson.NgaySinh.Date != newborn.NgaySinh!.Value.Date
        || await db.ThanhVienHos.CountAsync(x => x.NhanKhauId == newPersonId && x.DenNgay == null && !x.LaChuHo && x.TuNgay == DateOnly.FromDateTime(newborn.NgayDangKy!.Value)) != 1)
        throw new Exception("Birth must create a child and one active household membership, with optional identity number");
    var repeatedBirth = await populationService.CreateKhaiSinhAsync(newborn, "birth-officer");
    if (!repeatedBirth.Success || repeatedBirth.Data?.NhanKhauId != newPersonId
        || await db.NhanKhaus.CountAsync(x => x.HoTen == newborn.HoTen) != 1) throw new Exception("Retry duplicated birth or child");
    var birthList = await populationService.GetBienDongsAsync(newborn.HoTen, 1, 10, (int)Service.TanAn.Domain.Enums.LoaiBienDongEnum.KhaiSinh);
    if (birthList.Data?.Items.Single().HoSoKhaiSinh?.Cha?.HoTen != "Test person") throw new Exception("Saved birth declaration did not round-trip through list");
    var anotherChild = BirthForm("Trẻ không có định danh thứ hai");
    if (!(await populationService.CreateKhaiSinhAsync(anotherChild, "birth-officer")).Success) throw new Exception("Multiple children without identity must be allowed");
    foreach (var bad in new[] { "missing-house", "future-birth", "early-registration", "identity", "same-parents", "missing-parent-name", "unknown-member" })
    {
        var invalidBirth = BirthForm("Invalid birth");
        switch (bad)
        {
            case "missing-house": invalidBirth.HoGiaDinhId = Guid.NewGuid(); break;
            case "future-birth": invalidBirth.NgaySinh = DateTime.Today.AddDays(1); break;
            case "early-registration": invalidBirth.NgayDangKy = invalidBirth.NgaySinh!.Value.AddDays(-1); break;
            case "identity": invalidBirth.SoDinhDanh = "123"; break;
            case "same-parents": invalidBirth.Me = invalidBirth.Cha; break;
            case "missing-parent-name": invalidBirth.Cha!.HoTen = ""; break;
            case "unknown-member": invalidBirth.NguoiYeuCauId = Guid.NewGuid(); break;
        }
        if ((await populationService.CreateKhaiSinhAsync(invalidBirth, "birth-officer")).Success) throw new Exception($"Invalid birth accepted: {bad}");
    }
    var duplicateIdentity = BirthForm("Duplicate identity"); duplicateIdentity.SoDinhDanh = "999999999999";
    if ((await populationService.CreateKhaiSinhAsync(duplicateIdentity, "birth-officer")).Success) throw new Exception("Birth accepted duplicate identity");
    var beforeFailedBirth = await db.NhanKhaus.CountAsync();
    var beforeFailedChanges = await db.BienDongDanCus.CountAsync();
    var beforeFailedAudit = await db.AuditLogs.CountAsync();
    await using (var failedDb = new TanAnDbContext(options))
    {
        var failedService = new Service.TanAn.Application.Services.PopulationService(failedDb, new FailingBirthAudit(new Service.TanAn.Application.Services.AuditLogService(failedDb)));
        try { await failedService.CreateKhaiSinhAsync(BirthForm("Must roll back"), "birth-officer"); throw new Exception("Expected birth audit failure"); }
        catch (BirthAuditFailure) { }
    }
    if (await db.NhanKhaus.CountAsync() != beforeFailedBirth || await db.BienDongDanCus.CountAsync() != beforeFailedChanges || await db.AuditLogs.CountAsync() != beforeFailedAudit)
        throw new Exception("Birth failure left partial child, change or audit rows");
    Console.WriteLine("PASS: birth declaration snapshots, child/membership creation, optional identity, retry deduplication, validation and atomic rollback");
    var createdBenefit = await welfareService.CreateDoiTuongAnSinhAsync(new() { NhanKhauId = createdPerson.Data.Id, LoaiDoiTuong = Service.TanAn.Domain.Enums.DoiTuongAnSinhEnum.NguoiCaoTuoi, MucTroCapHangThang = 500000, NgayBatDauHuong = DateTime.Today }, "ui-test");
    if (!createdChange.Success || !createdBenefit.Success || createdBenefit.Data == null) throw new Exception("Create change/benefit failed");
    var payment = await welfareService.AddLichSuTroCapAsync(new() { DoiTuongAnSinhId = createdBenefit.Data.Id, ThangNam = "09/2026", SoTien = 500000 }, "ui-test");
    if (!payment.Success || payment.Data?.NguoiChiTra != "ui-test") throw new Exception("Payment save failed");
    Console.WriteLine("PASS: operational create actions persist household, resident, change, benefit and payout with actor");
    await KhaiSinhDraftChecks.Run(options);
}

catch (Exception ex)
{
    // Report a failed check without invoking the Windows unhandled-exception dialog.
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}

sealed class BirthAuditFailure : Exception;
sealed class FailingBirthAudit(Service.TanAn.Application.Interfaces.IAuditLogService inner) : Service.TanAn.Application.Interfaces.IAuditLogService
{
    public Task LogAsync(string username, string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null)
        => action == "Đăng ký khai sinh" ? throw new BirthAuditFailure() : inner.LogAsync(username, action, entityName, entityId, oldValues, newValues, ipAddress);
    public Task<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<AuditLog>>> GetAuditLogsAsync(string? keyword, int pageIndex, int pageSize)
        => inner.GetAuditLogsAsync(keyword, pageIndex, pageSize);
}
