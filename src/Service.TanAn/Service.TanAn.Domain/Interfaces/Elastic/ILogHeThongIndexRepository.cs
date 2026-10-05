// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;
using Service.Shared.Commons.Models;

namespace Service.TanAn.Domain.Interfaces.Elastic
{
    public interface ILogHeThongIndexRepository
    {
        Task<int> GetCountData(object query);
        Task<DataTableJson> GetPaged(object query);
        Task<object?> GetByIdAsync(string id);
    }
}
