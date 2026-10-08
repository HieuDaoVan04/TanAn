// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using PermissionDto = Service.Shared.Contracts.DTOs.PermissionDto;
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
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="permissionService"></param>
        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Tạo mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] PermissionForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _permissionService.CreateAsync(form);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Lấy danh sách phân trang
        /// </summary>
        [HttpPost("getpaged")]
        public async Task<ActionResult<DataTableJson>> GetPaged([FromBody] PermissionQuery query)
        {
            var result = await _permissionService.GetPaged(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách phân trang cho gán quyền
        /// </summary>
        [HttpPost("getpagedforganquyen")]
        public async Task<ActionResult<DataTableJson>> GetPagedForGanQuyen([FromBody] PermissionQuery query)
        {
            var result = await _permissionService.GetPagedForGanQuyen(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách phân trang dạng cây
        /// </summary>
        [HttpPost("getpagedtree")]
        public async Task<ActionResult<DataTableJson>> GetPagedTree([FromBody] PermissionQuery query)
        {
            var result = await _permissionService.GetPagedTree(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PermissionDto>> GetById(Guid id)
        {
            var dto = await _permissionService.GetByIdAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PermissionForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _permissionService.UpdateAsync(id, form);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Xóa
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _permissionService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Đồng bộ permission từ enum
        /// </summary>
        [HttpPost("sync-from-enum")]
        public async Task<IActionResult> SyncFromEnum()
        {
            var result = await _permissionService.SyncPermissionFromEnum();
            return Ok(result);
        }

        /// <summary>
        /// Duyệt theo ID
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _permissionService.ChangeModerationStatusAsync(id, ModerationStatus.Approved);
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
            var success = await _permissionService.ChangeModerationStatusAsync(id, ModerationStatus.Rejected);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Chuyển về chưa duyệt (tương thích endpoint cũ)
        /// </summary>
        [HttpPut("{id}/pending-review")]
        public async Task<IActionResult> PendingReview(Guid id)
        {
            var success = await _permissionService.ChangeModerationStatusAsync(id, ModerationStatus.PendingReview);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Chuyển về chưa duyệt (tương thích endpoint cũ)
        /// </summary>
        [HttpPut("{id}/pending-approval")]
        public async Task<IActionResult> PendingApproval(Guid id)
        {
            var success = await _permissionService.ChangeModerationStatusAsync(id, ModerationStatus.PendingApproval);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Hủy theo ID
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _permissionService.ChangeModerationStatusAsync(id, ModerationStatus.Cancelled);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
