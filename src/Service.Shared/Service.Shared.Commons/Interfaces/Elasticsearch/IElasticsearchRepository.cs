// "Một sản phẩm của HieuDV"

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Shared.Commons.Interfaces.Elasticsearch
{
    public interface IElasticsearchRepository<T> where T : class
    {
        Task<bool> IndexAsync(T document);
        Task<bool> IndexManyAsync(IEnumerable<T> documents);
        Task<T?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
    }
}
