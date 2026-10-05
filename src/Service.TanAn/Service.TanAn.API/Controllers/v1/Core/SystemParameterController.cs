// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;

namespace Service.TanAn.API.Controllers.v1.Core
{
    /// <summary>
    /// Dich vu api SystemParameter
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    [TypeFilter(typeof(SystemParameterExceptionFilter))]
    public class SystemParameterController : ControllerBase
    {
        private readonly ISystemParameterService _service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public SystemParameterController(ISystemParameterService service)
        {
            _service = service;
        }

        /// <summary>
        /// Tạo mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] SystemParameterForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _service.CreateAsync(form);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Lấy danh sách phân trang
        /// </summary>
        [HttpPost("getpaged")]
        public async Task<ActionResult<DataTableJson>> GetPaged([FromBody] SystemParameterQuery query)
        {
            var result = await _service.GetPaged(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SystemParameterDto>> GetById(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Đồng bộ tham số hệ thống từ Enum
        /// </summary>
        [HttpPost("syncrsystemparameterfromenum")]
        public async Task<IActionResult> SyncSystemParameterFromEnum()
        {
            var updated = await _service.SyncSystemParameterFromEnum();
            if (!updated)
                return BadRequest("Đã xảy ra lỗi trong quá trình đồng bộ tham số hệ thống");
            return Ok();
        }

        /// <summary>
        /// Lấy chi tiết theo code
        /// </summary>
        [HttpGet("GetByCode/{Code}")]
        public async Task<ActionResult<SystemParameterDto>> GetByCode(string Code)
        {
            var dto = await _service.GetByCodeAsync(Code);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SystemParameterForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, form);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Cập nhật giá trị theo code
        /// </summary>
        [HttpPut("UpdateValueByCode/{code}")]
        public async Task<IActionResult> UpdateValueByCodeAsync(string code, [FromBody] SystemParameterForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateValueByCodeAsync(code, form);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Duyệt theo ID
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _service.ChangeModerationStatusAsync(id, ModerationStatus.Approved);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Hủy duyệt theo ID
        /// </summary>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _service.ChangeModerationStatusAsync(id, ModerationStatus.Rejected);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Xóa
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
