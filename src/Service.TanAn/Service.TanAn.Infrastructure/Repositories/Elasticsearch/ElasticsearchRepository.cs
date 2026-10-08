// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Interfaces.Elasticsearch;

namespace Service.TanAn.Infrastructure.Repositories.Elasticsearch
{
    public class ElasticsearchRepository<T> : IElasticsearchRepository<T> where T : class
    {
        public Task<bool> IndexAsync(T document)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IndexManyAsync(IEnumerable<T> documents)
        {
            return Task.FromResult(true);
        }

        public Task<T?> GetByIdAsync(string id)
        {
            return Task.FromResult<T?>(default);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return Task.FromResult(true);
        }
    }
}
