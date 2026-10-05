// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using Service.Shared.Commons.Models;
using Service.TanAn.Domain.Enums;

namespace Service.Shared.Contracts.DTOs
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? OTP { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public RoleEnum Role { get; set; }
        public string? ApThon { get; set; }
    }

    public class LoginDto
    {
        public string access_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string refresh_token { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public RoleEnum Role { get; set; } = RoleEnum.NguoiDan;
        public string? ApThon { get; set; }
    }
}
