// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Service.Shared.Commons.Extensions;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;

namespace Service.TanAn.API.Controllers.v1.Core
{
    /// <summary>
    /// Xác thực phân quyền
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : BaseController
    {
        /// <summary>
        /// Modal login 
        /// </summary>
        public class LoginRequest
        {
            /// <summary>
            /// Tài khoản
            /// </summary>
            public string? Username { get; set; }

            /// <summary>
            /// Mật khẩu
            /// </summary>
            public string? Password { get; set; }

            /// <summary>
            /// token active
            /// </summary>
            public string? Token { get; set; }

            /// <summary>
            /// Code otp 2FA
            /// </summary>
            public string? OTP { get; set; }
        }

        /// <summary>
        /// Modal login2A
        /// </summary>
        public class LoginRequest2A
        {
#pragma warning disable CA1707 // Identifiers should not contain underscores
            /// <summary>
            /// Grant_type
            /// </summary>
            [FromForm(Name = "grant_type")]
            public string? Grant_type { get; set; }

            /// <summary>
            /// Tài khoản
            /// </summary>
            [FromForm(Name = "client_id")]
            public string? Client_id { get; set; }

            /// <summary>
            /// Mật khẩu
            /// </summary>
            [FromForm(Name = "client_secret")]
            public string? Client_secret { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
        }

        /// <summary>
        /// Modal refresh token
        /// </summary>
        public class RefreshTokenRequest
        {
            /// <summary>
            /// refreshToken
            /// </summary>
            public string? RefreshToken { get; set; }

            /// <summary>
            /// ActiveToken
            /// </summary>
            public string? ActiveToken { get; set; }
        }

        /// <summary>
        /// Modal login 
        /// </summary>
        public class LogoutRequest
        {
            /// <summary>
            /// Token
            /// </summary>
            public string? Token { get; set; }
        }

        private readonly IAuthService _authService;

        /// <summary>
        /// Khởi tạo AuthController
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="requestContext"></param>
        public AuthController(IAuthService authService, IRequestContext requestContext)
            : base(requestContext)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="InvalidInputException"></exception>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                LoginDto? Output = new LoginDto();

                if (!string.IsNullOrEmpty(request.Username) && !string.IsNullOrEmpty(request.Password))
                {
                    Output = await _authService.DangNhap(request.Username, request.Password, request.OTP);

                    if (Output == null)
                    {
                        throw new InvalidInputException("Tên đăng nhập hoặc mật khẩu không chính xác.");
                    }
                }

                return Ok(Output);
            }
            catch (Exception ex)
            {
                throw new InvalidInputException(ex.Message);
            }
        }

        /// <summary>
        /// Get số liệu thống kê bàn làm việc
        /// </summary>
        [HttpGet("GetThongTinBLV")]
        public async Task<ActionResult> GetThongTinBLV()
        {
            try
            {
                List<LuotTruyCap> result = await _authService.GetThongTinBLV();
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new InvalidInputException(ex.Message);
            }
        }
    }
}
