// "Một sản phẩm của HieuDV"

using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    /// <summary>
    /// Interface dịch vụ xác thực tài khoản & đăng nhập hệ thống Tân An
    /// </summary>
    public interface IAuthService
    {
        Task<bool> Check2FAUser(string UserName, string Password);
        Task<LoginDto?> DangNhap(string username, string password, string? OTP);
        Task<LoginDto?> DangNhapDev(string username, string password, string? OTP);
        Task<List<LuotTruyCap>> GetThongTinBLV();
    }
}
