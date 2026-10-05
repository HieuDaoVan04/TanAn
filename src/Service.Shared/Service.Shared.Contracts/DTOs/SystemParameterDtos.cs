// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class SystemParameterDto : BaseEntiyDto
    {
        public int STT { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Description { get; set; }
        public bool IsSync { get; set; }
    }

    public class SystemParameterForm
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Description { get; set; }
        public bool IsSync { get; set; }
    }

    public class SystemParameterQuery : BaseQuery
    {
        public string? Code { get; set; }
        public string? Value { get; set; }
    }
}
