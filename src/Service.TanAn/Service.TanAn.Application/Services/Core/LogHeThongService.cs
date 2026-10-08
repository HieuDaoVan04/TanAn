// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Interfaces.Elastic;

namespace Service.TanAn.Application.Services.Core
{
    public class LogHeThongService : ILogHeThongService
    {
        private readonly ILogHeThongIndexRepository _logsHeThongRepository;

        public LogHeThongService(ILogHeThongIndexRepository logsHeThongRepository)
        {
            _logsHeThongRepository = logsHeThongRepository ?? throw new ArgumentNullException(nameof(logsHeThongRepository));
        }

        public async Task<DataTableJson> GetPagedLogHeThongAsync(LogHeThongQuery query)
        {
            DataTableJson dataTableJson = await _logsHeThongRepository.GetPaged(query);
            return dataTableJson;
        }

        public async Task<LogHeThongDto?> GetByIdLogHeThongAsync(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new ArgumentException("Id không thể thiếu.", nameof(Id));
            }

            var res = await _logsHeThongRepository.GetByIdAsync(Id);
            if (res == null)
            {
                throw new KeyNotFoundException($"Log with Id {Id} not found.");
            }
            return res as LogHeThongDto;
        }
    }
}
