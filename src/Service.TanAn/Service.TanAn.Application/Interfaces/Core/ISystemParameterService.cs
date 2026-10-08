// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface ISystemParameterService
    {
        Task<Guid> CreateAsync(SystemParameterForm item);
        Task<bool> DeleteAsync(Guid Id);
        Task<bool> UpdateAsync(Guid Id, SystemParameterForm item);
        Task<DataTableJson> GetPaged(BaseQuery baseQuery);
        Task<SystemParameterDto> GetByIdAsync(Guid Id);
        Task<bool> SyncSystemParameterFromEnum();
        Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus);
        Task<bool> UpdateValueByCodeAsync(string Code, SystemParameterForm item);
        Task<SystemParameterDto> GetByCodeAsync(string Code);
    }
}
