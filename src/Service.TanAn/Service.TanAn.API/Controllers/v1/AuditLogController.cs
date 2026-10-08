using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.Shared.Commons.Models;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<AuditLog>>>> GetLogs([FromQuery] string? keyword, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var res = await _auditLogService.GetAuditLogsAsync(keyword, pageIndex, pageSize);
            return Ok(res);
        }

        [HttpPost("xuat-excel")]
        public async Task<IActionResult> XuatExcel([FromBody] BaseQuery? query)
        {
            var res = await _auditLogService.GetAuditLogsAsync(query?.Keyword, 1, 100000);
            var list = res.Data?.Items ?? new List<AuditLog>();
            var columns = new List<(string Header, Func<AuditLog, object?> Selector)>
            {
                ("Thời Gian", x => x.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")),
                ("Cán Bộ Thao Tác", x => x.Username),
                ("Hành Động", x => x.Action),
                ("Đối Tượng", x => x.EntityName),
                ("Mã Định Danh", x => x.EntityId),
                ("Giá Trị Cũ", x => x.OldValues),
                ("Giá Trị Mới / Mô Tả", x => x.NewValues),
                ("Địa Chỉ IP", x => x.IpAddress)
            };
            var bytes = Service.Shared.Commons.Helpers.ExcelExportHelper.ExportToExcel("Nhật Ký Hệ Thống", list, columns);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NhatKyHeThong_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
}


