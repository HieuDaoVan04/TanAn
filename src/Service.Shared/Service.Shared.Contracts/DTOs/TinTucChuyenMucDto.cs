// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class TinTucChuyenMucDto
    {
        public Guid Id { get; set; }
        public Guid? ChaId { get; set; }
        public string Ten { get; set; } = string.Empty;
        public bool DaGan { get; set; }
    }

    public class TinTucChuyenMucQuery
    {
        public Guid ChuyenTrangId { get; set; }
        public Guid UserId { get; set; }
        public GridRequest gridRequest { get; set; } = new();
    }
}
