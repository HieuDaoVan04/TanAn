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
    /// Log thao tác người dùng API
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LogThaoTacNguoiDungController : BaseController
    {
        private readonly ILogThaoTacNguoiDungService _service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="service"></param>
        public LogThaoTacNguoiDungController(IRequestContext requestContext, ILogThaoTacNguoiDungService service)
            : base(requestContext)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy chi tiết theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LogThaoTacNguoiDungDto>> GetById(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Lấy danh sách phân trang
        /// </summary>
        [HttpPost("GetPaged")]
        public async Task<ActionResult<DataTableJson>> GetPaged([FromBody] LogThaoTacNguoiDungQuery query)
        {
            var result = await _service.GetPaged(query);
            return Ok(result);
        }
    }
}
