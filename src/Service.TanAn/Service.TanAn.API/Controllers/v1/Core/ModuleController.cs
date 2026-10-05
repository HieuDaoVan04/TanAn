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
    /// Controller xử lý các Module trong hệ thống.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ModuleController : BaseController
    {
        private readonly IModuleService _moduleService;

        /// <summary>
        /// Khởi tạo constructor.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="moduleService"></param>
        public ModuleController(IRequestContext requestContext, IModuleService moduleService)
            : base(requestContext)
        {
            _moduleService = moduleService;
        }

        /// <summary>
        /// Lấy tất cả module
        /// </summary>  
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var module = _moduleService.GetAll();
            return Ok(module);
        }

        /// <summary>
        /// Lấy thông tin Role theo ID.
        /// </summary>
        /// <param name="id">ID của Role</param>
        /// <returns>Thông tin chi tiết Role</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var module = await _moduleService.GetByIdAsync(id);
            return Ok(module);
        }

        /// <summary>
        /// Lấy danh sách Role theo phân trang.
        /// </summary>
        /// <param name="searchOptions">Điều kiện tìm kiếm và phân trang</param>
        /// <returns>Danh sách Role</returns>
        [HttpPost("GetPaged")]
        public IActionResult GetPaged([FromBody] BaseQuery searchOptions)
        {
            var result = _moduleService.GetPaged(searchOptions);
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới một Role.
        /// </summary>
        /// <param name="request">Thông tin Role cần thêm</param>
        /// <returns>ID của Role vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ModuleForm request)
        {
            var newId = await _moduleService.AddAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = newId }, request);
        }

        /// <summary>
        /// Cập nhật thông tin Role.
        /// </summary>
        /// <param name="id">ID của Role</param>
        /// <param name="request">Thông tin Role cần cập nhật</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ModuleForm request)
        {
            var success = await _moduleService.UpdateAsync(id, request);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Xóa Role theo ID.
        /// </summary>
        /// <param name="id">ID của Role cần xóa</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _moduleService.DeleteAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Duyệt Role theo ID.
        /// </summary>
        /// <param name="id">ID của Role cần duyệt</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _moduleService.ApproveAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Hủy duyệt Role theo ID.
        /// </summary>
        /// <param name="id">ID của Role cần hủy duyệt</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var success = await _moduleService.RejectAsync(id);
            if (!success)
                return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Lấy tất cả module theo cây lọc
        /// </summary>
        /// <param name="phanLoai">Phân loại menu</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách module lọc theo phân loại và từ khóa</returns>
        [HttpGet("get-tree")]
        public IActionResult GetTree([FromQuery] EnumModuleType phanLoai, [FromQuery] string searchTerm = "")
        {
            var result = _moduleService.GetFilteredTree((int)phanLoai, searchTerm);

            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả module theo cây lọc
        /// </summary>
        /// <param name="phanLoai">Phân loại menu</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="RoleID">ID Role</param>
        /// <returns>Danh sách module lọc theo phân loại và từ khóa</returns>
        [HttpGet("get-tree-theo-role")]
        public IActionResult GetTreeTheoRole([FromQuery] EnumModuleType phanLoai, [FromQuery] Guid RoleID, [FromQuery] string searchTerm = "")
        {
            var result = _moduleService.GetFilteredTreePhanQuyen((int)phanLoai, searchTerm, RoleID);

            return Ok(result);
        }

        /// <summary>
        /// Lấy tất cả module mà đã duyệt theo phân loại để build ra giao diện CMS
        /// </summary>
        /// <param name="phanLoai">Phân loại menu</param>
        /// <returns>Danh sách module lọc theo phân loại </returns>
        [HttpGet("get-tree-publish")]
        public IActionResult GetTree([FromQuery] EnumModuleType phanLoai)
        {
            var result = _moduleService.GetTreePublishingForMCS(phanLoai);

            return Ok(result);
        }
    }
}
