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
    /// Dich vu api Users
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public UsersController(IUsersService service)
        {
            _service = service;
        }

        /// <summary>
        /// Tạo mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] UsersForm form)
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
        public ActionResult<DataTableJson> GetPaged([FromBody] UserQuery query)
        {
            var result = _service.GetPaged(query);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách phân trang theo mã đơn vị
        /// </summary>
        [HttpPost("getpagedbygroupid")]
        public async Task<ActionResult<DataTableJson>> GetPagedByGroupId([FromBody] UserQuery query)
        {
            if (query.GroupId == Guid.Empty)
                return BadRequest("GroupId không được để trống.");
            var result = await _service.GetPagedByGroupId(query);
            return Ok(result);
        }

        /// <summary>
        /// Gán người dùng vào group
        /// </summary>
        [HttpPut("gannguoidungvaogroup/{UserId}/{GroupId}")]
        public async Task<ActionResult<DataTableJson>> GanNguoiDungVaoGroup(Guid UserId, Guid GroupId)
        {
            var result = await _service.AddUserToGroupAsync(UserId, GroupId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết theo Id
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id)
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UsersForm form)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, form);
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

        /// <summary>
        /// Xóa người dùng khỏi group
        /// </summary>
        [HttpDelete("removefromgroup/{UserId}/{Groupid}")]
        public async Task<IActionResult> XoaNguoiDungKhoiGroup(Guid UserId, Guid Groupid)
        {
            var deleted = await _service.RemoveUserFromGroupAsync(UserId, Groupid);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
