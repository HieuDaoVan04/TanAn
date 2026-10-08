using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.API.Authentication;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Application.Services;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BienDongController : ControllerBase
    {
        private readonly IPopulationService _popService;

        public BienDongController(IPopulationService popService)
        {
            _popService = popService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<BienDongDto>>>> GetList([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] int? loaiBienDong = null, [FromQuery] DateTime? tuNgay = null, [FromQuery] DateTime? denNgay = null, [FromQuery] Guid? apThonId = null)
        {
            var res = await _popService.GetBienDongsAsync(keyword, pageIndex, pageSize, loaiBienDong, tuNgay, denNgay, apThonId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<BienDongDto>>> Create([FromBody] CreateBienDongForm form)
        {
            string user = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var res = await _popService.CreateBienDongAsync(form, user);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("xuat-excel")]
        public async Task<IActionResult> XuatExcel([FromBody] BienDongQuery? query)
        {
            var res = await _popService.GetBienDongsAsync(query?.Keyword, 1, int.MaxValue, query?.LoaiBienDong, query?.TuNgay, query?.DenNgay, query?.ApThonId);
            if (!res.Success) return BadRequest(res);
            var list = res.Data?.Items ?? new List<BienDongDto>();
            var columns = new List<(string Header, Func<BienDongDto, object?> Selector)>
            {
                ("Loại Biến Động", b => b.LoaiBienDong.ToString()),
                ("Họ Tên Nhân Khẩu", b => b.HoTenNhanKhau),
                ("Số CCCD", b => b.CCCDNhanKhau),
                ("Ngày Phát Sinh", b => b.NgayPhatSinh.ToString("dd/MM/yyyy")),
                ("Nơi Đến / Đi", b => b.NoiDenOrDi),
                ("Lý Do", b => b.LyDo),
                ("Cán Bộ Ghi Nhận", b => b.CanBoGhiNhan)
            };
            var bytes = Service.Shared.Commons.Helpers.ExcelExportHelper.ExportToExcel("Biến Động Dân Cư", list, columns);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BienDongDanCu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }

        [Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
        [HttpGet("khai-sinh/ho-gia-dinh")]
        public async Task<IActionResult> LookupBirthHousehold([FromQuery] string maSoHo, [FromServices] ITanAnDbContext db)
        {
            if (!await CanRegisterBirth(db)) return Forbid();
            var result = await _popService.GetHoGiaDinhByCodeAsync(maSoHo);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
        [HttpPost("khai-sinh")]
        public async Task<IActionResult> CreateBirth([FromBody, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever] KhaiSinhForm form, [FromServices] ITanAnDbContext db)
        {
            if (!await CanRegisterBirth(db)) return Forbid();
            return Conflict(new { message = "Khai sinh đã chuyển sang nhập hồ sơ nháp. Sử dụng /api/v1/BienDong/khai-sinh/ho-so; chưa thực hiện tạo nhân khẩu ở bước này." });
        }

        [Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
        [HttpGet("khai-sinh/ho-so")]
        public async Task<IActionResult> BirthDrafts([FromServices] ITanAnDbContext db, string? keyword = null, int pageIndex = 1, int pageSize = 10, Guid? apThonId = null, DateTime? tuNgay = null, DateTime? denNgay = null, bool choDuyet = false)
        {
            if (!await CanRegisterBirth(db)) return Forbid();
            var result = await _popService.GetKhaiSinhDraftsAsync(keyword, pageIndex, pageSize, apThonId, tuNgay, denNgay, choDuyet);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
        [HttpGet("khai-sinh/ho-so/{id:guid}")]
        public async Task<IActionResult> BirthDraft(Guid id, [FromServices] ITanAnDbContext db)
        {
            if (!await CanRegisterBirth(db)) return Forbid();
            var result = await _popService.GetKhaiSinhDraftAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
        [HttpPost("khai-sinh/ho-so")]
        public async Task<IActionResult> SaveBirthDraft([FromBody] JsonElement payload, [FromServices] ITanAnDbContext db)
        {
            if (!await CanRegisterBirth(db)) return Forbid();
            // Form đăng ký hoàn tất có Required; nháp dùng bộ kiểm tra riêng, không chạy MVC validation của form hoàn tất.
            KhaiSinhDraftSaveForm? request;
            try { request = payload.Deserialize<KhaiSinhDraftSaveForm>(new JsonSerializerOptions(JsonSerializerDefaults.Web)); }
            catch (JsonException) { return BadRequest(new { message = "Dữ liệu hồ sơ không đúng định dạng." }); }
            if (request?.HoSo == null) return BadRequest(new { message = "Thiếu nội dung hồ sơ." });
            var result = await _popService.SaveKhaiSinhDraftAsync(request, User.FindFirstValue(ClaimTypes.Name)!);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize(AuthenticationSchemes = LoginSessionAuthenticationHandler.SchemeName)]
        [HttpPost("khai-sinh/ho-so/{id:guid}/duyet")]
        public async Task<IActionResult> ApproveBirth(Guid id, [FromBody] KhaiSinhApproveForm request, [FromServices] ITanAnDbContext db)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                || !await BirthApprovalAuthorization.CanApproveAsync(db, userId)) return Forbid();
            var result = await _popService.ApproveKhaiSinhAsync(id, request.PhienBan, User.FindFirstValue(ClaimTypes.Name)!);
            return result.Success ? Ok(result) : Conflict(result);
        }

        private async Task<bool> CanRegisterBirth(ITanAnDbContext db)
        {
            if (User.IsInRole("Admin")) return true;
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return false;
            return await (from ur in db.UserRoles
                          join role in db.Roles on ur.RoleId equals role.Id
                          join grant in db.RoleModules on role.Id equals grant.RoleId
                          join module in db.Modules on grant.ModuleId equals module.Id
                          where ur.UserId == userId && role.ModerationStatus == ModerationStatus.Approved
                            && module.ModerationStatus == ModerationStatus.Approved && module.LienKet == "/bien-dong/khai-sinh"
                            && (module.PhanHeId == null || module.PhanHe!.HoatDong)
                          select grant.ModuleId).AnyAsync();
        }
    }
}
