// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class ChuyenTrangDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool DaGan { get; set; }
    }

    public class ChuyenTrangQuery
    {
        public Guid UserId { get; set; }
        public GridRequest gridRequest { get; set; } = new();
    }

    public class PhanChuyenTrangDto
    {
        public Guid UserId { get; set; }
        public List<Guid> ChuyenTrangIds { get; set; } = new();
    }
}
