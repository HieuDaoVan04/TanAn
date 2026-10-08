// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Interfaces;

namespace Service.TanAn.Application.Services.Core
{
    public class LogThaoTacNguoiDungService : ILogThaoTacNguoiDungService
    {
        private readonly IUnitOfWorkQuanTriHeThong _UnitOfWork;

        public LogThaoTacNguoiDungService(IUnitOfWorkQuanTriHeThong unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }

        public async Task<LogThaoTacNguoiDungDto?> GetByIdAsync(Guid id)
        {
            var entity = await _UnitOfWork.LogThaoTacNguoiDungRepository
                .GetByIdAsync(id);

            if (entity == null) return null;

            return new LogThaoTacNguoiDungDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ModuleName = entity.ModuleName,
                Action = entity.Action,
                Created = entity.Created,
                BeforeChange = entity.BeforeChange,
                AfterChange = entity.AfterChange,
                IPAddress = entity.IPAddress,
                Description = entity.Description
            };
        }

        public async Task<DataTableJson> GetPaged(LogThaoTacNguoiDungQuery query)
        {
            var pagedObj = await _UnitOfWork.LogThaoTacNguoiDungRepository
                                          .GetPagedDtoAsync(query);

            var (items, total) = ((List<LogThaoTacNguoiDungDto> Items, int Total))pagedObj;

            return new DataTableJson(items.ConvertAll(x => (object)x),
                                     total,
                                     query.PageIndex,
                                     query.PageSize);
        }
    }
}
