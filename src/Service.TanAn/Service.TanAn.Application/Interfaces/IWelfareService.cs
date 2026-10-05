using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces
{
    public interface IWelfareService
    {
        Task<ApiResult<PagedResult<DoiTuongAnSinhDto>>> GetDoiTuongAnSinhsAsync(string? keyword, int? loaiDoiTuong, int pageIndex, int pageSize);
        Task<ApiResult<DoiTuongAnSinhDto>> CreateDoiTuongAnSinhAsync(CreateAnSinhForm form, string username);
        Task<ApiResult<LichSuTroCapDto>> AddLichSuTroCapAsync(CreateTroCapForm form, string username);
    }
}


