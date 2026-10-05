// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class LogThaoTacNguoiDungDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? ModuleName { get; set; }
        public string? Action { get; set; }
        public DateTime? Created { get; set; }
        public string? BeforeChange { get; set; }
        public string? AfterChange { get; set; }
        public string? IPAddress { get; set; }
        public string? Description { get; set; }
    }

    public class LogThaoTacNguoiDungQuery : BaseQuery
    {
        public string? UserId { get; set; }
        public string? ModuleName { get; set; }
        public string? Action { get; set; }
        public DateTime? SearchTuNgay { get; set; }
        public DateTime? SearchDenNgay { get; set; }
    }
}
