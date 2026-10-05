// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface ILogThaoTacNguoiDungService
    {
        Task<LogThaoTacNguoiDungDto?> GetByIdAsync(Guid id);
        Task<DataTableJson> GetPaged(LogThaoTacNguoiDungQuery query);
    }
}
