// "Một sản phẩm của HieuDV"

using System;

namespace Service.TanAn.Domain.Entities.TimKiem
{
    public class DuLieuTinKiemIndex
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string SiteTag { get; set; } = "portal";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
