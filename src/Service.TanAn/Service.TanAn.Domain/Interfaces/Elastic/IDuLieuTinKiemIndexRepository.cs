// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Interfaces.Elasticsearch;
using Service.Shared.Commons.Models;
using Service.TanAn.Domain.Entities.TimKiem;

namespace Service.TanAn.Domain.Interfaces.Elastic
{
    public interface IDuLieuTinKiemIndexRepository : IElasticsearchRepository<DuLieuTinKiemIndex>
    {
        /// <summary>
        /// Xóa theo list Id. dù có lỗi hoặc ko tồn tại cũng ko sao? ko nhả ra lỗi
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        Task DeleteBulk(List<Guid> sourceId);

        /// <summary>
        /// Xóa theo Id. dù có lỗi hoặc ko tồn tại cũng ko sao? ko nhả ra lỗi
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        Task DeleteByIdAsync(Guid sourceId);

        /// <summary>Index tin tức</summary>
        Task<bool> IndexNewsAsync(List<Guid> sourceIds, string SiteTag = "portal");

        /// <summary>
        /// index tin tức
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="SiteTag"></param>
        /// <returns></returns>
        Task<bool> IndexNewsAsync(Guid sourceId, string SiteTag = "portal");

        /// <summary>Index video</summary>
        Task<bool> IndexVideoAsync(Guid sourceId, string SiteTag = "portal");

        /// <summary>Index hình ảnh</summary>
        Task<bool> IndexImageAsync(Guid sourceId, string SiteTag = "portal");

        /// <summary>Index văn bản quy phạm pháp luật</summary>
        Task<bool> IndexVanBanQpplAsync(Guid sourceId);

        /// <summary>Index văn bản chỉ đạo điều hành</summary>
        Task<bool> IndexVanBanCddhAsync(Guid sourceId);

        /// <summary>Tìm kiếm theo điều kiện cụ thể</summary>
        Task<(List<DuLieuTinKiemIndex> Items, long Total)> SearchAsync(PublisingSearchQuery query);
    }
}
