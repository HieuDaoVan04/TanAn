// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface IJwtService
    {
        /// <summary>
        /// Tạo token JWT từ SsoUserInfo (claims).
        /// </summary>
        Task<string> GenerateToken(SsoUserInfo user);
    }
}
