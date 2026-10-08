// "Một sản phẩm của HieuDV"

using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginDto?> DangNhap(string username, string password, string? OTP = null);
        Task<LoginDto?> DangNhapDev(string username, string password, string? OTP = null);
        Task<bool> Check2FAUser(string UserName, string Password);
        Task<List<LuotTruyCap>> GetThongTinBLV();

        // Tương thích với UI Blazor hiện tại
        Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResult<bool>> RegisterAsync(RegisterRequest request);
    }
}
