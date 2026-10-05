// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces.MSSQL
{
    public interface IUserGroupsRepository : IRepository<UserGroups>
    {
        Task<UserGroups?> FindAsync(Expression<Func<UserGroups, bool>> predicate);
        Task<List<UserGroups>> FindAllAsync(Expression<Func<UserGroups, bool>> predicate);
        void Delete(UserGroups entity);
    }
}
