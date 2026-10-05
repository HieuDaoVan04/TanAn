// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
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
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public RoleController(IRoleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Tạo mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] RoleForm form)
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
        public async Task<ActionResult<DataTableJson>> GetPaged([FromBody] BaseQuery query)
        {
            var result = await _service.GetPaged(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách phân trang gán quyền
        /// </summary>
        [HttpPost("GetPagedForGanQuyen/{id:guid}")]
        public ActionResult<DataTableJson> GetPagedForGanQuyen(Guid id, [FromBody] RoleQuery query)
        {
            var result = _service.GetPagedForGanQuyen(id, query);
            return Ok(result);
        }

        /// <summary>
        /// Gắn menu cho vai trò
        /// </summary>
        [HttpPost("GanMenu")]
        public async Task<ActionResult<Guid>> GanMenu([FromBody] GanMenuVaoVaiTroDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.GanMenuAsync(item);
            if (!result)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Lấy chi tiết theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoleDto>> GetById(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Lấy chi tiết lịch sử phân vai trò theo Id
        /// </summary>
        [HttpGet("getuserrolehistory/{id:guid}")]
        public async Task<ActionResult<UserRoleHistoryDto>> GetUserRoleHistoryById(Guid id)
        {
            var dto = await _service.GetUserRoleHistoryById(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Cập nhật 
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoleForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, form);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Đồng bộ vai trò từ Enum
        /// </summary>
        [HttpGet("syncrolefromenum")]
        public async Task<IActionResult> SyncRoleFromEnum()
        {
            var updated = await _service.SyncRoleFromEnum();
            if (!updated)
                return BadRequest("Đã xảy ra lỗi trong quá trình đồng bộ vai trò");
            return Ok();
        }

        /// <summary>
        /// Gắn quyền cho vai trò
        /// </summary>
        [HttpPost("ganquyenvaovaitro/{RoleId}")]
        public async Task<IActionResult> GanQuyenVaoVaiTro(Guid RoleId, [FromBody] GanQuyenDto request)
        {
            var updated = await _service.GanQuyenVaoVaiTro(RoleId, request);
            if (!updated)
                return BadRequest("Đã xảy ra lỗi trong quá trình gắn quyền cho vai trò");
            return Ok();
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

        /// <summary>
        /// Lấy danh sách quyền từ ID vai trò
        /// </summary>
        [HttpGet("getpermissionfromroleid/{Id}")]
        public async Task<IActionResult> GetPermissionFromRoleId(Guid Id)
        {
            if (Id == Guid.Empty)
                return BadRequest("ID không hợp lệ");
            var result = await _service.GetPermissionFromRoleId(Id);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách module từ ID vai trò
        /// </summary>
        [HttpGet("getmodulefromroleid/{Id}")]
        public async Task<IActionResult> GetModuleFromRoleId(Guid Id)
        {
            if (Id == Guid.Empty)
                return BadRequest("ID không hợp lệ");
            var result = await _service.GetModuleFromRoleId(Id);
            return Ok(result);
        }
    }
}
