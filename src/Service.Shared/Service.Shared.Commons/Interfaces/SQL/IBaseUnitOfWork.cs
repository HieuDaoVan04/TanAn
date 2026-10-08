// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Shared.Commons.Interfaces.SQL
{
    public interface IBaseUnitOfWork : IDisposable
    {
        int Complete(Guid UserId = default, Guid DepartmentId = default);
        Task<int> CompleteAsync(Guid UserId = default, Guid DepartmentId = default);
        IEnumerable<T> ExecuteSqlRaw<T>(string sql) where T : class;
        Task<ITransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> ExecuteNonQueryAsync(string sql);
        Task<int> ExecuteNonQueryInterpolatedAsync(FormattableString sql);
    }
}
