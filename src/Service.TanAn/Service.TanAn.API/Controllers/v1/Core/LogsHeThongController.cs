// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;

namespace Service.TanAn.API.Controllers.v1.Core
{
    /// <summary>
    /// Danh mục Logs
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LogsHeThongController : BaseController
    {
        private readonly ILogHeThongService _LogsHeThongService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="LogsHeThongService"></param>
        public LogsHeThongController(IRequestContext requestContext, ILogHeThongService LogsHeThongService) : base(requestContext)
        {
            _LogsHeThongService = LogsHeThongService;
        }

        /// <summary>
        /// Lấy theo ID log hệ thống
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/GetByIdLogHeThong")]
        public async Task<IActionResult> GetLogHeThong(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Id không thể thiếu");
            var odata = await _LogsHeThongService.GetByIdLogHeThongAsync(id);
            return Ok(odata);
        }

        /// <summary>
        /// Get danh sách phân trang
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost("GetPagedLogHeThong")]
        public async Task<IActionResult> GetPagedLogHeThong([FromBody] LogHeThongQuery searchOptions)
        {
            if (searchOptions == null)
            {
                throw new ArgumentNullException(nameof(searchOptions), "Đầu vào không hợp lệ");
            }

            DataTableJson data = await _LogsHeThongService.GetPagedLogHeThongAsync(searchOptions);
            return Ok(data);
        }
    }
}
