// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Services.Core
{
    public class GroupsService : IGroupsService
    {
        private static readonly List<Groups> _groups = new List<Groups>
        {
            new Groups
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                GroupCode = "ADMIN_GROUP",
                GroupName = "Nhóm Quản trị hệ thống",
                Description = "Nhóm người dùng có quyền quản trị toàn bộ hệ thống",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-30),
                CreatedBy = "System"
            },
            new Groups
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                GroupCode = "CANBO_GROUP",
                GroupName = "Nhóm Cán bộ Xã",
                Description = "Nhóm cán bộ thực thi quy trình nghiệp vụ",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-20),
                CreatedBy = "System"
            },
            new Groups
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                GroupCode = "NGUOIDAN_GROUP",
                GroupName = "Nhóm Người dân",
                Description = "Nhóm công dân tra cứu và nộp thủ tục hành chính",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-10),
                CreatedBy = "System"
            }
        };

        public Task<Guid> CreateAsync(GroupsForm item)
        {
            var entity = new Groups
            {
                Id = item.Id != Guid.Empty ? item.Id : Guid.NewGuid(),
                GroupCode = item.GroupCode,
                GroupName = item.GroupName,
                Description = item.Description,
                IsActive = item.IsActive,
                CreatedDate = DateTime.Now,
                CreatedBy = "HieuDV"
            };

            _groups.Add(entity);
            return Task.FromResult(entity.Id);
        }

        public Task<bool> DeleteAsync(Guid Id)
        {
            var entity = _groups.FirstOrDefault(x => x.Id == Id);
            if (entity == null) return Task.FromResult(false);

            _groups.Remove(entity);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(Guid Id, GroupsForm item)
        {
            var entity = _groups.FirstOrDefault(x => x.Id == Id);
            if (entity == null) return Task.FromResult(false);

            entity.GroupCode = item.GroupCode;
            entity.GroupName = item.GroupName;
            entity.Description = item.Description;
            entity.IsActive = item.IsActive;

            return Task.FromResult(true);
        }

        public Task<GroupsDto?> GetByIdAsync(Guid Id)
        {
            var entity = _groups.FirstOrDefault(x => x.Id == Id);
            if (entity == null) return Task.FromResult<GroupsDto?>(null);

            var dto = MapToDto(entity);
            return Task.FromResult<GroupsDto?>(dto);
        }

        public Task<DataTableJson<GroupsDto>> GetDataTableAsync(GroupsQuery query)
        {
            return GetPaged(query);
        }

        public Task<DataTableJson<GroupsDto>> GetPaged(GroupsQuery query)
        {
            var q = _groups.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.GroupCode))
            {
                q = q.Where(x => x.GroupCode.Contains(query.GroupCode, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.GroupName))
            {
                q = q.Where(x => x.GroupName.Contains(query.GroupName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                q = q.Where(x => x.GroupName.Contains(query.Keyword, StringComparison.OrdinalIgnoreCase) ||
                                 x.GroupCode.Contains(query.Keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (query.IsActive.HasValue)
            {
                q = q.Where(x => x.IsActive == query.IsActive.Value);
            }

            int total = q.Count();
            int pageIndex = query.PageIndex > 0 ? query.PageIndex : 1;
            int pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = q.Skip((pageIndex - 1) * pageSize)
                         .Take(pageSize)
                         .Select(MapToDto)
                         .ToList();

            return Task.FromResult(new DataTableJson<GroupsDto>(items, total, pageIndex, pageSize));
        }

        public Task<DataTableJson<GroupsDto>> GetPagedTreeView(GroupsQuery query)
        {
            return GetPaged(query);
        }

        public Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus moderationStatus)
        {
            var entity = _groups.FirstOrDefault(x => x.Id == id);
            if (entity == null) return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<List<GroupsDto>> GetAllAsync()
        {
            var list = _groups.Select(MapToDto).ToList();
            return Task.FromResult(list);
        }

        private static GroupsDto MapToDto(Groups entity)
        {
            return new GroupsDto
            {
                Id = entity.Id,
                GroupCode = entity.GroupCode,
                GroupName = entity.GroupName,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                CreatedBy = entity.CreatedBy
            };
        }
    }
}
