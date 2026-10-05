using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResult<ThongKeTongQuanDto>> GetThongKeTongQuanAsync();
    }
}


