// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface ILogHeThongService
    {
        Task<LogHeThongDto?> GetByIdLogHeThongAsync(string id);
        Task<DataTableJson> GetPagedLogHeThongAsync(LogHeThongQuery searchOptions);
    }
}
