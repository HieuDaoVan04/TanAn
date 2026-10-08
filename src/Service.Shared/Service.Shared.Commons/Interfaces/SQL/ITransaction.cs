// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;

namespace Service.Shared.Commons.Interfaces.SQL
{
    public interface ITransaction : IDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
