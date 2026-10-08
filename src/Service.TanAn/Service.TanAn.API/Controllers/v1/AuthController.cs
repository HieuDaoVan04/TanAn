// "Một sản phẩm của HieuDV"

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResult<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            var res = await _authService.LoginAsync(request);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpPost("dang-nhap")]
        public async Task<ActionResult<LoginDto>> DangNhap(string username, string password, string? OTP = null)
        {
            var res = await _authService.DangNhap(username, password, OTP);
            if (res == null) return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
            return Ok(res);
        }

        [HttpPost("dang-nhap-dev")]
        public async Task<ActionResult<LoginDto>> DangNhapDev(string username, string password, string? OTP = null)
        {
            var res = await _authService.DangNhapDev(username, password, OTP);
            if (res == null) return BadRequest();
            return Ok(res);
        }

        [HttpGet("thong-tin-blv")]
        public async Task<ActionResult<List<LuotTruyCap>>> GetThongTinBLV()
        {
            var data = await _authService.GetThongTinBLV();
            return Ok(data);
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResult<bool>>> Register([FromBody] RegisterRequest request)
        {
            var res = await _authService.RegisterAsync(request);
            if (!res.Success) return BadRequest(res);
            return Ok(res);
        }
    }
}
