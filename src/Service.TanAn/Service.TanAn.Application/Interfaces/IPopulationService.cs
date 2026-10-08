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
        Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByCodeAsync(string maSoHo);
        Task<ApiResult<HoGiaDinhDto>> CreateHoGiaDinhAsync(CreateHoGiaDinhForm form, string username);
        
        Task<ApiResult<PagedResult<NhanKhauDto>>> GetNhanKhausAsync(string? keyword, string? apThon, int pageIndex, int pageSize, Guid? groupId = null);
        Task<ApiResult<NhanKhauDto>> GetNhanKhauByIdAsync(Guid id);
        Task<ApiResult<NhanKhauDto>> CreateNhanKhauAsync(CreateNhanKhauForm form, string username);
        Task<ApiResult<NhanKhauDto>> UpdateNhanKhauAsync(Guid id, CreateNhanKhauForm form, string username);
        
        Task<ApiResult<PagedResult<BienDongDto>>> GetBienDongsAsync(string? keyword, int pageIndex, int pageSize, int? loaiBienDong = null, DateTime? tuNgay = null, DateTime? denNgay = null, Guid? apThonId = null);
        Task<ApiResult<BienDongDto>> CreateBienDongAsync(CreateBienDongForm form, string username);
        Task<ApiResult<BienDongDto>> CreateKhaiSinhAsync(KhaiSinhForm form, string username);
        Task<ApiResult<KhaiSinhDraftDto>> SaveKhaiSinhDraftAsync(KhaiSinhDraftSaveForm request, string username);
        Task<ApiResult<KhaiSinhDraftDto>> ApproveKhaiSinhAsync(Guid id, int phienBan, string username);
        Task<ApiResult<KhaiSinhDraftDto>> GetKhaiSinhDraftAsync(Guid id);
        Task<ApiResult<PagedResult<KhaiSinhDraftDto>>> GetKhaiSinhDraftsAsync(string? keyword, int pageIndex, int pageSize, Guid? apThonId = null, DateTime? tuNgay = null, DateTime? denNgay = null, bool choDuyet = false);
        Task<ApiResult<PagedResult<KhaiSinhDraftDto>>> GetKhaiSinhApprovalNotificationsAsync(string username, int limit = 10);
    }
}
