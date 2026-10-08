using System;

namespace Service.TanAn.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Approve, Reject
        public string EntityName { get; set; } = string.Empty; // NhanKhau, HoGiaDinh, YeuCau, etc.
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
    }
}

