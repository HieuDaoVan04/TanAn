// "Một sản phẩm của HieuDV"

using System;

namespace Service.TanAn.Domain.Entities
{
    public class LogThaoTacNguoiDung
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public string? ModuleName { get; set; }
        public string? Action { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? BeforeChange { get; set; }
        public string? AfterChange { get; set; }
        public string? IPAddress { get; set; }
        public string? Description { get; set; }
    }
}
