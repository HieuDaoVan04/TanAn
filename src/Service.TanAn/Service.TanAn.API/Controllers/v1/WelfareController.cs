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
    public class WelfareController : ControllerBase
    {
        private readonly IWelfareService _welfareService;

        public WelfareController(IWelfareService welfareService)
        {
            _welfareService = welfareService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<DoiTuongAnSinhDto>>>> GetList([FromQuery] string? keyword, [FromQuery] int? loaiDoiTuong, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var res = await _welfareService.GetDoiTuongAnSinhsAsync(keyword, loaiDoiTuong, pageIndex, pageSize);
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<DoiTuongAnSinhDto>>> Create([FromBody] CreateAnSinhForm form)
        {
            string user = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var res = await _welfareService.CreateDoiTuongAnSinhAsync(form, user);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("tro-cap")]
        public async Task<ActionResult<ApiResult<LichSuTroCapDto>>> AddTroCap([FromBody] CreateTroCapForm form)
        {
            string user = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var res = await _welfareService.AddLichSuTroCapAsync(form, user);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("xuat-excel")]
        public async Task<IActionResult> XuatExcel([FromBody] BaseQuery? query)
        {
            var res = await _welfareService.GetDoiTuongAnSinhsAsync(query?.Keyword, null, 1, 100000);
            var list = res.Data?.Items ?? new List<DoiTuongAnSinhDto>();
            var columns = new List<(string Header, Func<DoiTuongAnSinhDto, object?> Selector)>
            {
                ("Họ Tên Đối Tượng", x => x.HoTen),
                ("Số CCCD", x => x.CCCD),
                ("Thôn / Ấp", x => x.ApThon),
                ("Phân Loại An Sinh", x => x.LoaiDoiTuong.ToString()),
                ("Trợ Cấp / Tháng (VNĐ)", x => x.MucTroCapHangThang),
                ("Ngày Bắt Đầu Hưởng", x => x.NgayBatDauHuong.ToString("dd/MM/yyyy")),
                ("Trạng Thái", x => x.TrangThaiHoatDong ? "Đang hưởng" : "Tạm dừng"),
                ("Ghi Chú", x => x.GhiChu)
            };
            var bytes = Service.Shared.Commons.Helpers.ExcelExportHelper.ExportToExcel("An Sinh Xã Hội", list, columns);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AnSinhXaHoi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}


