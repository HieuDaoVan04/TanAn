// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.TanAn.Domain.Entities.TimKiem;
using Service.TanAn.Domain.Interfaces.Elastic;

namespace Service.TanAn.Infrastructure.Repositories.Elasticsearch
{
    public class DuLieuTinKiemIndexRepository : ElasticsearchRepository<DuLieuTinKiemIndex>, IDuLieuTinKiemIndexRepository
    {
        public Task DeleteBulk(List<Guid> sourceId)
        {
            return Task.CompletedTask;
        }

        public Task DeleteByIdAsync(Guid sourceId)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IndexNewsAsync(List<Guid> sourceIds, string SiteTag = "portal")
        {
            return Task.FromResult(true);
        }

        public Task<bool> IndexNewsAsync(Guid sourceId, string SiteTag = "portal")
        {
            return Task.FromResult(true);
        }

        public Task<bool> IndexVideoAsync(Guid sourceId, string SiteTag = "portal")
        {
            return Task.FromResult(true);
        }

        public Task<bool> IndexImageAsync(Guid sourceId, string SiteTag = "portal")
        {
            return Task.FromResult(true);
        }

        public Task<bool> IndexVanBanQpplAsync(Guid sourceId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IndexVanBanCddhAsync(Guid sourceId)
        {
            return Task.FromResult(true);
        }

        public Task<(List<DuLieuTinKiemIndex> Items, long Total)> SearchAsync(PublisingSearchQuery query)
        {
            var items = new List<DuLieuTinKiemIndex>
            {
                new DuLieuTinKiemIndex
                {
                    Id = Guid.NewGuid(),
                    Title = "Thông báo họp HĐND Xã Tân An nhiệm kỳ 2026",
                    Summary = "Tóm tắt nội dung cuộc họp HĐND Xã Tân An",
                    Type = "News",
                    SiteTag = query.SiteTag ?? "portal",
                    CreatedAt = DateTime.Now
                }
            };

            return Task.FromResult((items, (long)items.Count));
        }
    }
}
