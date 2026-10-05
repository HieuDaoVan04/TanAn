// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface ILogThaoTacNguoiDungRepository : IRepository<LogThaoTacNguoiDung>
    {
        Task<LogThaoTacNguoiDung?> GetByIdAsync(Guid id);
        Task<object> GetPagedDtoAsync(object query);
    }
}
