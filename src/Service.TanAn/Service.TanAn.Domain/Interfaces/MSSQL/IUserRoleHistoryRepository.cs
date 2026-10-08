// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IUserRoleHistoryRepository : IRepository<UserRoleHistory>
    {
        Task<UserRoleHistory?> GetByIdAsync(Guid id);
    }
}
