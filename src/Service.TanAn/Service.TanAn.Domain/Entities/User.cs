// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Domain.Enums;

namespace Service.TanAn.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = string.Empty;
        public string Username { get => UserName; set => UserName = value; }
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Phone { get => PhoneNumber; set => PhoneNumber = value; }
        public RoleEnum Role { get; set; } = RoleEnum.CanBoXa;
        public string? ApThon { get; set; }
        public string? AvatarUrl { get; set; }
        public int PhanLoai { get; set; }
        public string? KeyPublic { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? LastLoginIp { get; set; }
        public int TotalLogin { get; set; }
        public int TotalLoginFaild { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get => Created; set => Created = value; }
        public DateTime? LastModified { get; set; }
        public Guid LastModifiedBy { get; set; }
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Approved;
    }
}
