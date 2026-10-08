using System;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces
{
    public interface ICitizenRequestService
    {
        Task<ApiResult<PagedResult<YeuCauDto>>> GetYeuCausAsync(string? keyword, int? trangThai, int pageIndex, int pageSize);
        Task<ApiResult<YeuCauDto>> CreateYeuCauAsync(CreateYeuCauForm form);
        Task<ApiResult<YeuCauDto>> UpdateYeuCauStatusAsync(UpdateYeuCauStatusForm form);
    }
}


