using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces
{
    public interface IPopulationService
    {
        Task<ApiResult<PagedResult<HoGiaDinhDto>>> GetHoGiaDinhsAsync(string? keyword, string? apThon, int pageIndex, int pageSize, Guid? groupId = null);
        Task SetChuHoAsync(Guid householdId, Guid residentId, string username);
        Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByIdAsync(Guid id);
        Task<ApiResult<HoGiaDinhDto>> CreateHoGiaDinhAsync(CreateHoGiaDinhForm form, string username);
        
        Task<ApiResult<PagedResult<NhanKhauDto>>> GetNhanKhausAsync(string? keyword, string? apThon, int pageIndex, int pageSize, Guid? groupId = null);
        Task<ApiResult<NhanKhauDto>> GetNhanKhauByIdAsync(Guid id);
        Task<ApiResult<NhanKhauDto>> CreateNhanKhauAsync(CreateNhanKhauForm form, string username);
        Task<ApiResult<NhanKhauDto>> UpdateNhanKhauAsync(Guid id, CreateNhanKhauForm form, string username);
        
        Task<ApiResult<PagedResult<BienDongDto>>> GetBienDongsAsync(string? keyword, int pageIndex, int pageSize);
        Task<ApiResult<BienDongDto>> CreateBienDongAsync(CreateBienDongForm form, string username);
    }
}


