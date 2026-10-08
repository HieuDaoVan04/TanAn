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
    /// Dich vu api
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupsService _service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public GroupsController(IGroupsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Tạo mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] GroupsForm form)
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
        public async Task<ActionResult<DataTableJson<GroupsDto>>> GetPaged([FromBody] GroupsQuery query)
        {
            var result = await _service.GetPaged(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách phân trang dạng cây
        /// </summary>
        [HttpPost("getpagedtree")]
        public async Task<ActionResult<DataTableJson<GroupsDto>>> GetPagedTree([FromBody] GroupsQuery query)
        {
            var result = await _service.GetPagedTreeView(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GroupsDto>> GetById(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GroupsForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, form);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Duyệt theo ID.
        /// </summary>
        /// <param name="id">ID của cần duyệt</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _service.ChangeModerationStatusAsync(id, ModerationStatus.Approved);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Hủy duyệt theo ID.
        /// </summary>
        /// <param name="id">ID của cần hủy duyệt</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _service.ChangeModerationStatusAsync(id, ModerationStatus.Rejected);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Xóa dịch vụ
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
