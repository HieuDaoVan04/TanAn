using Service.Shared.Commons.Model.SQL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Services;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Infrastructure.Persistence;
using Service.TanAn.Infrastructure.Services;

static class KhaiSinhDraftChecks
{
    public static async Task Run(DbContextOptions<TanAnDbContext> options)
    {
        await using var db = new TanAnDbContext(options);
        var officer = new User { UserName = "draft-check", Role = RoleEnum.CanBoThon };
        var other = new User { UserName = "other-draft-check", Role = RoleEnum.CanBoThon };
        var village = new ApThon { Ma = "DRAFT-A", Ten = "Thôn nháp A" };
        var outside = new ApThon { Ma = "DRAFT-B", Ten = "Thôn nháp B" };
        var house = new HoGiaDinh { MaSoHo = "DRAFT-HOUSE", TenChuHo = "Chủ hộ nháp", DiaBan = village };
        db.Users.AddRange(officer, other); db.ApThons.Add(outside); db.HoGiaDinhs.Add(house);
        db.PhuTrachThons.Add(new() { User = officer, Thon = village });
        await db.SaveChangesAsync();
        var counts = (await db.NhanKhaus.CountAsync(), await db.ThanhVienHos.CountAsync(), await db.BienDongDanCus.CountAsync(), await db.YeuCauNguoiDans.CountAsync());
        var service = new PopulationService(db, new AuditLogService(db));
        var request = new KhaiSinhDraftSaveForm { HoSo = new() { KieuNhap = KieuNhapKhaiSinh.TuNhap, NgayDangKy = null, HoTen = "Trẻ nháp", LoaiNoiSinh = LoaiNoiSinh.NuocNgoai, QuocGiaNoiSinh = "Pháp", ThanhPhoNoiSinh = "Paris", TinhTrangCha = TinhTrangThongTinChaMe.ChuaXacDinh } };
        var saved = await service.SaveKhaiSinhDraftAsync(request, officer.UserName);
        Check(saved.Success && saved.Data?.PhienBan == 1 && saved.Data.HoSo?.HoGiaDinhId == Guid.Empty, "manual incomplete draft without household");
        Check(saved.Data?.ModerationStatus == ModerationStatus.Pending && (await db.HoSoKhaiSinhs.AsNoTracking().SingleAsync(x => x.Id == saved.Data!.Id)).ModerationStatus == ModerationStatus.Pending,
            "new declarations default to shared Pending status in DTO and database");
        var retry = await service.SaveKhaiSinhDraftAsync(request, officer.UserName);
        Check(retry.Success && retry.Data?.Id == saved.Data!.Id && await db.HoSoKhaiSinhs.CountAsync() == 1, "draft creation retry does not duplicate");
        var reopened = (await service.GetKhaiSinhDraftAsync(saved.Data!.Id)).Data!;
        Check(reopened.HoSo?.QuocGiaNoiSinh == "Pháp" && reopened.HoSo.TinhTrangCha == TinhTrangThongTinChaMe.ChuaXacDinh, "structured fields round-trip on reopen");
        reopened.HoSo!.HoTen = "Trẻ nháp đã sửa";
        var edited = await service.SaveKhaiSinhDraftAsync(new() { HoSo = reopened.HoSo, PhienBan = reopened.PhienBan }, officer.UserName);
        Check(edited.Success && edited.Data?.PhienBan == 2, "update draft preserves ID and increments version");
        reopened.HoSo.HoTen = "Stale overwrite";
        Check(!(await service.SaveKhaiSinhDraftAsync(new() { HoSo = reopened.HoSo, PhienBan = 1 }, officer.UserName)).Success, "stale edits rejected");
        Check((await service.GetKhaiSinhDraftAsync(saved.Data.Id)).Data?.HoTenTre == "Trẻ nháp đã sửa", "stale edit leaves saved data intact");
        foreach (var invalid in new KhaiSinhForm[] { new() { SoDinhDanh = "123" }, new() { NgaySinh = DateTime.Today.AddDays(1) }, new() { HoTen = new string('a', 201) }, new() { LoaiNoiSinh = (LoaiNoiSinh)99 }, new() { HoGiaDinhId = Guid.NewGuid() } })
            Check(!(await service.SaveKhaiSinhDraftAsync(new() { HoSo = invalid }, officer.UserName)).Success, "invalid entered draft value rejected");
        Check(!(await service.SaveKhaiSinhDraftAsync(new() { ApThonId = outside.Id }, officer.UserName)).Success, "unassigned draft village rejected");
        var linked = await service.SaveKhaiSinhDraftAsync(new() { HoSo = new() { HoGiaDinhId = house.Id, MaSoHo = "FAKE", TenChuHo = "FAKE" } }, officer.UserName);
        Check(linked.Success && linked.Data?.MaSoHo == house.MaSoHo && linked.Data.ApThonId == village.Id, "linked household derives canonical code and village");
        var page = await service.GetKhaiSinhDraftsAsync("nháp đã sửa", 1, 1);
        Check(page.Success && page.Data?.TotalCount == 1 && page.Data.Items.Count == 1 && page.Data.Items[0].HoSo == null, "paged draft list filters without exposing entire JSON");
        Check((await service.GetKhaiSinhDraftsAsync(null, 1, 1, village.Id)).Data?.TotalCount == 1, "village filters apply before paging");
        Check((await service.GetKhaiSinhDraftsAsync(null, 1, 10, tuNgay:DateTime.Today, denNgay:DateTime.Today)).Data?.TotalCount == 2, "inclusive draft creation date filter");
        var actor = new DataActor(new HttpContextAccessor()) { Principal = Principal(officer.Id) };
        await using (var scoped = new TanAnDbContext(options, actor))
        {
            var scopedService = new PopulationService(scoped, new AuditLogService(scoped));
            Check((await scopedService.GetKhaiSinhDraftAsync(saved.Data.Id)).Success, "creator can reopen draft without village");
            actor.Principal = Principal(other.Id);
            Check(!(await scopedService.GetKhaiSinhDraftAsync(saved.Data.Id)).Success && await scoped.HoSoKhaiSinhs.CountAsync() == 0, "another officer cannot read unknown-village or unassigned drafts");
            scoped.Attach(new HoSoKhaiSinh { Id = saved.Data.Id, NguoiTaoId = other.Id, MaHoSo = "FORGED", NoiDungJson = "{}" }).State = EntityState.Modified;
            try { await scoped.SaveChangesAsync(); throw new Exception("Attached draft scope bypass"); }
            catch (UnauthorizedAccessException) { scoped.ChangeTracker.Clear(); }
            actor.Principal = Principal(officer.Id);
            await using (var revoke = new TanAnDbContext(options)) { revoke.PhuTrachThons.Remove(await revoke.PhuTrachThons.SingleAsync(x => x.UserId == officer.Id)); await revoke.SaveChangesAsync(); }
            Check(!(await scopedService.GetKhaiSinhDraftAsync(linked.Data!.Id)).Success, "revoking village assignment immediately hides linked drafts");
            Check((await scopedService.GetKhaiSinhDraftAsync(saved.Data.Id)).Success, "own draft without village remains accessible");
        }
        var failed = new KhaiSinhDraftSaveForm { HoSo = new() { HoTen = "Rollback nháp" } };
        await using (var failureDb = new TanAnDbContext(options))
        {
            var failService = new PopulationService(failureDb, new FailingDraftAudit(new AuditLogService(failureDb)));
            try { await failService.SaveKhaiSinhDraftAsync(failed, officer.UserName); throw new Exception("Expected draft audit failure"); }
            catch (DraftAuditFailure) { }
        }
        Check(!await db.HoSoKhaiSinhs.AnyAsync(x => x.Id == failed.HoSo.RequestId), "failed audit rolls back draft");
        Check(counts == (await db.NhanKhaus.CountAsync(), await db.ThanhVienHos.CountAsync(), await db.BienDongDanCus.CountAsync(), await db.YeuCauNguoiDans.CountAsync()), "draft writes never create residents, membership, changes or reception requests");
        var chairman = new User { UserName = "draft-chairman", Role = RoleEnum.ChuTichXa };
        db.Users.Add(chairman); await db.SaveChangesAsync();
        Check(!(await service.ApproveKhaiSinhAsync(saved.Data.Id, 2, officer.UserName)).Success, "approval service rejects account without ctx assignment");
        Check(!(await service.ApproveKhaiSinhAsync(saved.Data.Id, 2, chairman.UserName)).Success, "chairman account enum alone does not grant approval");
        var ctxRole = new Role { RoleName = "Chủ tịch xã", RoleCode = "CTX" };
        var birthMenu = new Module { TenModule = "Khai sinh", LienKet = "/bien-dong/khai-sinh" };
        db.Roles.Add(ctxRole); db.Modules.Add(birthMenu);
        db.UserRoles.Add(new() { UserId = chairman.Id, RoleId = ctxRole.Id });
        await db.SaveChangesAsync();
        Check(!(await service.ApproveKhaiSinhAsync(saved.Data.Id, 2, chairman.UserName)).Success, "ctx role still needs birth menu access");
        db.RoleModules.Add(new() { RoleId = ctxRole.Id, ModuleId = birthMenu.Id });
        ctxRole.ModerationStatus = ModerationStatus.Pending; await db.SaveChangesAsync();
        Check(!(await service.ApproveKhaiSinhAsync(saved.Data.Id, 2, chairman.UserName)).Success, "pending ctx role cannot approve");
        ctxRole.ModerationStatus = ModerationStatus.Approved; await db.SaveChangesAsync();
        Check(!(await service.GetKhaiSinhApprovalNotificationsAsync(officer.UserName)).Success, "pending approval notifications require ctx and birth menu permission");
        Check(!(await service.ApproveKhaiSinhAsync(saved.Data.Id, 2, chairman.UserName)).Success
            && (await service.GetKhaiSinhDraftAsync(saved.Data.Id)).Data?.ModerationStatus == ModerationStatus.Pending,
            "incomplete declaration cannot approve or create a resident");
        ctxRole = await db.Roles.SingleAsync(x => x.Id == ctxRole.Id);
        var fullForm = (await service.GetKhaiSinhDraftAsync(saved.Data.Id)).Data!.HoSo!;
        fullForm.HoGiaDinhId = house.Id; fullForm.NgaySinh = new(2024, 1, 1); fullForm.NgayDangKy = new(2024, 1, 2);
        fullForm.NoiSinh = "Paris"; fullForm.QueQuan = "Tân An"; fullForm.ThuongTru = village.Ten; fullForm.QuanHeVoiChuHo = "Con";
        fullForm.HoTenNguoiYeuCau = "Người yêu cầu"; fullForm.SoGiayTo = "123456789012";
        fullForm.NoiCuTruNguoiYeuCau = village.Ten; fullForm.QuanHeVoiTre = "Cha"; fullForm.SoDinhDanh = "333333333333";
        var completed = await service.SaveKhaiSinhDraftAsync(new() { HoSo = fullForm, PhienBan = 2 }, chairman.UserName);
        Check(completed.Success, "officer can complete household and required fields before approval");
        var rollbackForm = System.Text.Json.JsonSerializer.Deserialize<KhaiSinhForm>(System.Text.Json.JsonSerializer.Serialize(fullForm))!;
        rollbackForm.RequestId = linked.Data!.Id; rollbackForm.HoTen = "Trẻ kiểm tra rollback"; rollbackForm.SoDinhDanh = "";
        var rollbackDraft = await service.SaveKhaiSinhDraftAsync(new() { HoSo = rollbackForm, PhienBan = 1 }, chairman.UserName);
        Check(rollbackDraft.Success, "rollback fixture has complete birth data");
        actor.Principal = Principal(chairman.Id);
        await using (var scoped = new TanAnDbContext(options, actor))
        {
            var scopedService = new PopulationService(scoped, new AuditLogService(scoped));
            Check((await scopedService.GetKhaiSinhDraftAsync(saved.Data.Id)).Success && (await scopedService.GetKhaiSinhDraftAsync(linked.Data!.Id)).Success,
                "chairman can read manual and linked declarations across the commune");
            var pendingBefore = await scopedService.GetKhaiSinhApprovalNotificationsAsync(chairman.UserName, 1);
            Check(pendingBefore.Success && pendingBefore.Data?.TotalCount == 2 && pendingBefore.Data.Items.Count == 1
                && pendingBefore.Data.Items.All(x => x.ModerationStatus == ModerationStatus.Pending && x.HoSo == null),
                "pending notification count covers all pending declarations while preview is bounded and omits JSON");
            Check(!(await scopedService.ApproveKhaiSinhAsync(saved.Data.Id, 1, chairman.UserName)).Success, "service rejects approval of stale version");
            var approved = await scopedService.ApproveKhaiSinhAsync(saved.Data.Id, completed.Data!.PhienBan, chairman.UserName);
            Check(approved.Success && approved.Data?.ModerationStatus == ModerationStatus.Approved && approved.Data.DaGhiNhan && approved.Data.NguoiDuyet == chairman.UserName && approved.Data.NgayDuyet.HasValue,
                "chairman approval succeeds through scoped context");
            var birthRecord = await scoped.BienDongDanCus.SingleAsync(x => x.Id == saved.Data.Id);
            var child = await scoped.NhanKhaus.SingleAsync(x => x.Id == birthRecord.NhanKhauId);
            Check(child.MaHoGiaDinh == house.Id && child.HoTen == fullForm.HoTen && child.CCCD == fullForm.SoDinhDanh
                && await scoped.ThanhVienHos.CountAsync(x => x.NhanKhauId == child.Id && x.HoGiaDinhId == house.Id) == 1,
                "approval atomically creates child in selected household, membership and linked birth change");
            var pendingAfter = await scopedService.GetKhaiSinhApprovalNotificationsAsync(chairman.UserName);
            Check(pendingAfter.Data?.TotalCount == 1 && pendingAfter.Data.Items.All(x => x.Id != saved.Data.Id),
                "approved declaration immediately disappears from notification count and preview");
            var beforeAudit = await scoped.AuditLogs.CountAsync();
            Check((await scopedService.ApproveKhaiSinhAsync(saved.Data.Id, 2, chairman.UserName)).Success && await scoped.AuditLogs.CountAsync() == beforeAudit
                && await scoped.BienDongDanCus.CountAsync(x => x.Id == saved.Data.Id) == 1 && await scoped.NhanKhaus.CountAsync(x => x.CCCD == fullForm.SoDinhDanh) == 1,
                "approval retry keeps original audit and timestamp");
            Check(!(await scopedService.SaveKhaiSinhDraftAsync(new() { HoSo = approved.Data!.HoSo!, PhienBan = approved.Data.PhienBan }, chairman.UserName)).Success,
                "approved form cannot be saved again or changed");
        }
        await using (var failureDb = new TanAnDbContext(options))
        {
            var failService = new PopulationService(failureDb, new FailingDraftAudit(new AuditLogService(failureDb)));
            try { await failService.ApproveKhaiSinhAsync(linked.Data!.Id, rollbackDraft.Data!.PhienBan, chairman.UserName); throw new Exception("Expected approval audit failure"); }
            catch (DraftAuditFailure) { }
        }
        Check((await db.HoSoKhaiSinhs.AsNoTracking().Where(x => x.Id == linked.Data!.Id).Select(x => x.ModerationStatus).SingleAsync()) == ModerationStatus.Pending, "approval audit failure rolls back status");
        Check(!await db.NhanKhaus.AnyAsync(x => x.HoTen == rollbackForm.HoTen) && !await db.BienDongDanCus.AnyAsync(x => x.Id == linked.Data!.Id),
            "approval audit failure also rolls back child, membership and birth change");
        ctxRole.RoleCode = "OTHER"; await db.SaveChangesAsync();
        Check(!(await service.ApproveKhaiSinhAsync(linked.Data!.Id, 1, chairman.UserName)).Success, "changing ctx code immediately revokes service approval");
        ctxRole.RoleCode = "ctx"; await db.SaveChangesAsync();
        db.UserRoles.Remove(await db.UserRoles.SingleAsync(x => x.UserId == chairman.Id && x.RoleId == ctxRole.Id));
        await db.SaveChangesAsync();
        Check(!(await service.ApproveKhaiSinhAsync(linked.Data!.Id, 1, chairman.UserName)).Success, "removing ctx assignment immediately revokes service approval");
        Check(!(await service.GetKhaiSinhApprovalNotificationsAsync(chairman.UserName)).Success, "removing ctx assignment also revokes pending notifications");
        db.UserRoles.Add(new() { UserId = officer.Id, RoleId = ctxRole.Id }); await db.SaveChangesAsync();
        actor.Principal = Principal(officer.Id);
        await using (var scoped = new TanAnDbContext(options, actor))
        {
            var scopedService = new PopulationService(scoped, new AuditLogService(scoped));
            var notices = await scopedService.GetKhaiSinhApprovalNotificationsAsync(officer.UserName);
            Check(notices.Success && notices.Data?.TotalCount == 0, "ctx notifications respect village scope after village assignment is revoked");
        }
        Check((counts.Item1 + 1, counts.Item2 + 1, counts.Item3 + 1, counts.Item4) == (await db.NhanKhaus.CountAsync(), await db.ThanhVienHos.CountAsync(), await db.BienDongDanCus.CountAsync(), await db.YeuCauNguoiDans.CountAsync()),
            "one approved declaration creates exactly one person, membership and change without reception requests");
        db.UserRoles.Add(new() { UserId = chairman.Id, RoleId = ctxRole.Id }); await db.SaveChangesAsync();
        var duplicateForm = System.Text.Json.JsonSerializer.Deserialize<KhaiSinhForm>(System.Text.Json.JsonSerializer.Serialize(fullForm))!;
        duplicateForm.RequestId = Guid.NewGuid(); duplicateForm.HoTen = "Trẻ trùng định danh";
        var duplicateDraft = await service.SaveKhaiSinhDraftAsync(new() { HoSo = duplicateForm }, chairman.UserName);
        Check(duplicateDraft.Success && !(await service.ApproveKhaiSinhAsync(duplicateDraft.Data!.Id, 1, chairman.UserName)).Success
            && (await service.GetKhaiSinhDraftAsync(duplicateDraft.Data!.Id)).Data?.ModerationStatus == ModerationStatus.Pending
            && !await db.BienDongDanCus.AnyAsync(x => x.Id == duplicateDraft.Data!.Id),
            "duplicate identity rejects approval and rolls back status/version without creating a birth change");
        var legacyForm = System.Text.Json.JsonSerializer.Deserialize<KhaiSinhForm>(System.Text.Json.JsonSerializer.Serialize(fullForm))!;
        legacyForm.RequestId = Guid.NewGuid(); legacyForm.HoTen = "Trẻ hồ sơ đã duyệt cũ"; legacyForm.SoDinhDanh = ""; legacyForm.NgayDangKy = null;
        var legacyDraft = await service.SaveKhaiSinhDraftAsync(new() { HoSo = legacyForm }, chairman.UserName);
        var legacyRecord = await db.HoSoKhaiSinhs.SingleAsync(x => x.Id == legacyDraft.Data!.Id);
        var legacyTime = DateTime.UtcNow.AddDays(-1);
        legacyRecord.ModerationStatus = ModerationStatus.Approved; legacyRecord.NguoiDuyetId = officer.Id;
        legacyRecord.NguoiDuyet = officer.UserName; legacyRecord.NgayDuyet = legacyTime; legacyRecord.PhienBan++;
        await db.SaveChangesAsync();
        Check(!(await service.GetKhaiSinhDraftAsync(legacyRecord.Id)).Data!.DaGhiNhan, "legacy approved record exposes missing population registration");
        var legacyResult = await service.ApproveKhaiSinhAsync(legacyRecord.Id, legacyRecord.PhienBan, chairman.UserName);
        Check(legacyResult.Success && legacyResult.Data!.DaGhiNhan && legacyResult.Data.NguoiDuyet == officer.UserName
            && legacyResult.Data.NgayDuyet == legacyTime && await db.BienDongDanCus.CountAsync(x => x.Id == legacyRecord.Id) == 1,
            "completing legacy approval creates birth change and preserves original approver/time");
        var legacyBirth = await db.BienDongDanCus.AsNoTracking().SingleAsync(x => x.Id == legacyRecord.Id);
        var legacySnapshot = System.Text.Json.JsonSerializer.Deserialize<KhaiSinhForm>(legacyBirth.HoSoKhaiSinhJson!)!;
        Check(legacyResult.Data!.HoSo!.NgayDangKy?.Date == legacyTime.ToLocalTime().Date
            && legacySnapshot.NgayDangKy?.Date == legacyTime.ToLocalTime().Date
            && await db.ThanhVienHos.AnyAsync(x => x.NhanKhauId == legacyBirth.NhanKhauId && x.TuNgay == DateOnly.FromDateTime(legacyTime.ToLocalTime())),
            "missing registration date defaults to original approval date consistently in draft, birth snapshot and household membership");
        var completedLegacyVersion = legacyResult.Data!.PhienBan;
        Check((await service.ApproveKhaiSinhAsync(legacyRecord.Id, completedLegacyVersion, chairman.UserName)).Success
            && await db.NhanKhaus.CountAsync(x => x.HoTen == legacyForm.HoTen) == 1,
            "repeating legacy completion does not create another child");
        await CheckStatusMigration();
    }
    private static async Task CheckStatusMigration()
    {
        // Exercise the SQL emitted by the actual migration on both old status values.
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        async Task Sql(string text)
        {
            using var command = connection.CreateCommand(); command.CommandText = text;
            await command.ExecuteNonQueryAsync();
        }
        await Sql("""
            CREATE TABLE "HoSoKhaiSinhs" ("Id" INTEGER PRIMARY KEY, "DaDuyet" BOOLEAN NOT NULL, "NguoiDuyet" TEXT, "NgayDuyet" TEXT);
            INSERT INTO "HoSoKhaiSinhs" VALUES (1, 1, 'chairman', '2026-10-07'), (2, 0, NULL, NULL);
            """);
        var migration = new Service.TanAn.Infrastructure.Persistence.Migrations.UseBirthModerationStatus();
        var up = new Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite");
        var down = new Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder("Microsoft.EntityFrameworkCore.Sqlite");
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        migration.GetType().GetMethod("Up", flags)!.Invoke(migration, [up]);
        migration.GetType().GetMethod("Down", flags)!.Invoke(migration, [down]);
        foreach (var operation in up.Operations)
        {
            if (operation is Microsoft.EntityFrameworkCore.Migrations.Operations.AddColumnOperation column)
                await Sql($"ALTER TABLE \"HoSoKhaiSinhs\" ADD COLUMN \"{column.Name}\" INTEGER NOT NULL DEFAULT {column.DefaultValue};");
            else if (operation is Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation sql) await Sql(sql.Sql);
            else if (operation is Microsoft.EntityFrameworkCore.Migrations.Operations.DropColumnOperation columnToDrop)
                await Sql($"ALTER TABLE \"HoSoKhaiSinhs\" DROP COLUMN \"{columnToDrop.Name}\";");
        }
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT \"ModerationStatus\", \"NguoiDuyet\", \"NgayDuyet\" FROM \"HoSoKhaiSinhs\" ORDER BY \"Id\"";
            using var reader = await command.ExecuteReaderAsync();
            Check(await reader.ReadAsync() && reader.GetInt32(0) == (int)ModerationStatus.Approved && reader.GetString(1) == "chairman" && reader.GetString(2) == "2026-10-07",
                "status migration preserves approved declaration and approval metadata");
            Check(await reader.ReadAsync() && reader.GetInt32(0) == (int)ModerationStatus.Pending, "status migration preserves pending declaration");
        }
        foreach (var operation in down.Operations)
        {
            if (operation is Microsoft.EntityFrameworkCore.Migrations.Operations.AddColumnOperation)
                await Sql("ALTER TABLE \"HoSoKhaiSinhs\" ADD COLUMN \"DaDuyet\" BOOLEAN NOT NULL DEFAULT 0;");
            else if (operation is Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation sql) await Sql(sql.Sql);
            else if (operation is Microsoft.EntityFrameworkCore.Migrations.Operations.DropColumnOperation columnToDrop)
                await Sql($"ALTER TABLE \"HoSoKhaiSinhs\" DROP COLUMN \"{columnToDrop.Name}\";");
        }
        using var rollback = connection.CreateCommand();
        rollback.CommandText = "SELECT COUNT(*) FROM \"HoSoKhaiSinhs\" WHERE (\"Id\" = 1 AND \"DaDuyet\" = 1) OR (\"Id\" = 2 AND \"DaDuyet\" = 0)";
        Check(Convert.ToInt32(await rollback.ExecuteScalarAsync()) == 2, "rolling back status migration preserves both approval states");
    }
    private static ClaimsPrincipal Principal(Guid id) => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test"));
    private static void Check(bool result, string label) { if (!result) throw new Exception(label); Console.WriteLine("PASS: " + label); }
}

sealed class DraftAuditFailure : Exception;
sealed class FailingDraftAudit(Service.TanAn.Application.Interfaces.IAuditLogService inner) : Service.TanAn.Application.Interfaces.IAuditLogService
{
    public Task LogAsync(string username, string action, string entityName, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null)
        => entityName == "HoSoKhaiSinh" ? throw new DraftAuditFailure() : inner.LogAsync(username, action, entityName, entityId, oldValues, newValues, ipAddress);
    public Task<Service.Shared.Commons.Models.ApiResult<Service.Shared.Commons.Models.PagedResult<AuditLog>>> GetAuditLogsAsync(string? keyword, int pageIndex, int pageSize)
        => inner.GetAuditLogsAsync(keyword, pageIndex, pageSize);
}
