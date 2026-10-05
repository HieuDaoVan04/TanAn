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
    public class BienDongController : ControllerBase
    {
        private readonly IPopulationService _popService;

        public BienDongController(IPopulationService popService)
        {
            _popService = popService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<BienDongDto>>>> GetList([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var res = await _popService.GetBienDongsAsync(keyword, pageIndex, pageSize);
            return Ok(res);
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
        public async Task<IActionResult> XuatExcel([FromBody] BaseQuery? query)
        {
            var res = await _popService.GetBienDongsAsync(query?.Keyword, 1, 100000);
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
    }
}


