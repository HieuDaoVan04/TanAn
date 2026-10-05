using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NhanKhauController : ControllerBase
    {
        private readonly IPopulationService _popService;

        public NhanKhauController(IPopulationService popService)
        {
            _popService = popService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<PagedResult<NhanKhauDto>>>> GetList([FromQuery] string? keyword, [FromQuery] string? apThon, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var res = await _popService.GetNhanKhausAsync(keyword, apThon, pageIndex, pageSize);
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<NhanKhauDto>>> GetById(Guid id)
        {
            var res = await _popService.GetNhanKhauByIdAsync(id);
            if (!res.Success) return NotFound(res);
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult<NhanKhauDto>>> Create([FromBody] CreateNhanKhauForm form)
        {
            string user = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var res = await _popService.CreateNhanKhauAsync(form, user);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult<NhanKhauDto>>> Update(Guid id, [FromBody] CreateNhanKhauForm form)
        {
            string user = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var res = await _popService.UpdateNhanKhauAsync(id, form, user);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }
    }
}


