// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Service.Shared.Commons.Interfaces.SQL;

namespace Service.TanAn.Infrastructure.Persistence
{
    public class EfCoreTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;
        private bool _disposed;

        public EfCoreTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
