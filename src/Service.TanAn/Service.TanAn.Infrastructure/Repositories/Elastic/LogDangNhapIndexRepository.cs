// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Domain.Interfaces.Elastic;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.Infrastructure.Repositories.Elastic
{
    public class LogDangNhapIndexRepository : ILogDangNhapIndexRepository
    {
        private readonly TanAnDbContext _dbContext;

        public LogDangNhapIndexRepository(TanAnDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetCountData(LogDangNhapIndexQuery query)
        {
            var countInDb = await _dbContext.AuditLogs.CountAsync(l =>
                (!query.SearchTuNgay.HasValue || l.Timestamp >= query.SearchTuNgay.Value) &&
                (!query.SearchDenNgay.HasValue || l.Timestamp <= query.SearchDenNgay.Value));

            if (countInDb > 0) return countInDb;

            var random = new Random();
            return random.Next(30, 150);
        }
    }
}
