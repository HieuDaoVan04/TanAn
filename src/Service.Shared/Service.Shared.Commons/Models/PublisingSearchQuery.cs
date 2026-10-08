// "Một sản phẩm của HieuDV"

using System;

namespace Service.Shared.Commons.Models
{
    public class PublisingSearchQuery
    {
        public string Keyword { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? SiteTag { get; set; } = "tanan";
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
