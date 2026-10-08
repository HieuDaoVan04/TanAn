using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HoGiaDinhController : ControllerBase
    {
        private readonly IPopulationService _popService;

        public HoGiaDinhController(IPopulationService popService)
        {
            _popService = popService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<HoGiaDinhDto>>>> GetList([FromQuery] string? keyword, [FromQuery] string? apThon, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var res = await _popService.GetHoGiaDinhsAsync(keyword, apThon, pageIndex, pageSize);
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<HoGiaDinhDto>>> GetById(Guid id)
        {
            var res = await _popService.GetHoGiaDinhByIdAsync(id);
            if (!res.Success) return NotFound(res);
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<HoGiaDinhDto>>> Create([FromBody] CreateHoGiaDinhForm form)
        {
            string user = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var res = await _popService.CreateHoGiaDinhAsync(form, user);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("xuat-excel")]
        public async Task<IActionResult> XuatExcel([FromBody] BaseQuery? query)
        {
            var res = await _popService.GetHoGiaDinhsAsync(query?.Keyword, null, 1, 100000);
            var list = res.Data?.Items ?? new List<HoGiaDinhDto>();
            var columns = new List<(string Header, Func<HoGiaDinhDto, object?> Selector)>
            {
                ("Mã Sổ Hộ", x => x.MaSoHo),
                ("Tên Chủ Hộ", x => x.TenChuHo),
                ("Số CCCD", x => x.CCCDChuHo),
                ("Thôn / Ấp", x => x.ApThon),
                ("Địa Chỉ Cư Trú", x => x.DiaChi),
                ("Số Thành Viên", x => x.SoThanhVien),
                ("Ghi Chú", x => x.GhiChu)
            };
            var bytes = Service.Shared.Commons.Helpers.ExcelExportHelper.ExportToExcel("Sổ Hộ Khẩu", list, columns);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachHoGiaDinh_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}


