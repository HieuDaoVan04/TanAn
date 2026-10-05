// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces.Core
{
    public interface IGroupsService
    {
        /// <summary>
        /// Tạo mới bản ghi
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<Guid> CreateAsync(GroupsForm item);

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(Guid Id);

        /// <summary>
        /// Cập nhật bản ghi
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Guid Id, GroupsForm item);

        /// <summary>
        /// Xem chi tiết bản ghi
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<GroupsDto?> GetByIdAsync(Guid Id);

        /// <summary>
        /// Danh sách phân trang
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<DataTableJson<GroupsDto>> GetDataTableAsync(GroupsQuery query);

        /// <summary>
        /// Danh sách phân trang GetPaged
        /// </summary>
        Task<DataTableJson<GroupsDto>> GetPaged(GroupsQuery query);

        /// <summary>
        /// Danh sách phân trang dạng cây
        /// </summary>
        Task<DataTableJson<GroupsDto>> GetPagedTreeView(GroupsQuery query);

        /// <summary>
        /// Thay đổi trạng thái duyệt
        /// </summary>
        Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus);

        /// <summary>
        /// Lấy tất cả danh sách
        /// </summary>
        /// <returns></returns>
        Task<List<GroupsDto>> GetAllAsync();
    }
}
