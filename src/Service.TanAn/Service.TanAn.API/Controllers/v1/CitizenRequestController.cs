using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CitizenRequestController : ControllerBase
    {
        private readonly ICitizenRequestService _requestService;

        public CitizenRequestController(ICitizenRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<YeuCauDto>>>> GetList([FromQuery] string? keyword, [FromQuery] int? trangThai, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var res = await _requestService.GetYeuCausAsync(keyword, trangThai, pageIndex, pageSize);
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<YeuCauDto>>> Create([FromBody] CreateYeuCauForm form)
        {
            var res = await _requestService.CreateYeuCauAsync(form);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("status")]
        public async Task<ActionResult<ApiResult<YeuCauDto>>> UpdateStatus([FromBody] UpdateYeuCauStatusForm form)
        {
            if (string.IsNullOrWhiteSpace(form.CanBoXuLy))
            {
                form.CanBoXuLy = User.FindFirstValue(ClaimTypes.Name) ?? "Cán bộ tiếp nhận";
            }
            var res = await _requestService.UpdateYeuCauStatusAsync(form);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("xuat-excel")]
        public async Task<IActionResult> XuatExcel([FromBody] BaseQuery? query)
        {
            var res = await _requestService.GetYeuCausAsync(query?.Keyword, null, 1, 100000);
            var list = res.Data?.Items ?? new List<YeuCauDto>();
            var columns = new List<(string Header, Func<YeuCauDto, object?> Selector)>
            {
                ("Mã Hồ Sơ", r => r.MaYeuCau),
                ("Người Yêu Cầu", r => r.HoTenNguoiYeuCau),
                ("Số CCCD", r => r.CCCDNguoiYeuCau),
                ("Số Điện Thoại", r => r.SoDienThoai),
                ("Loại Thủ Tục", r => r.LoaiYeuCau),
                ("Nội Dung", r => r.NoiDung),
                ("Trạng Thái", r => r.TrangThai.ToString()),
                ("Cán Bộ Xử Lý", r => r.CanBoXuLy),
                ("Ngày Gửi", r => r.NgayGui.ToString("dd/MM/yyyy HH:mm:ss"))
            };
            var bytes = Service.Shared.Commons.Helpers.ExcelExportHelper.ExportToExcel("Dịch Vụ Công", list, columns);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"HoSoDichVuCong_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}


