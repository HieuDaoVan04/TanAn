// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public enum OrganizationUnitType
    {
        [Description("Đơn vị")]
        DonVi = 1,

        [Description("Phòng ban")]
        PhongBan = 2
    }

    public enum OrganizationUnitGroup
    {
        [Description("Nhóm mặc định")]
        Default = 1,

        [Description("Nhóm chuyên môn")]
        ChuyenMon = 2
    }

    public class GroupsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GroupName { get => Name; set => Name = value; }
        public string ShortName { get; set; } = string.Empty;
        public string IdentifierCode { get; set; } = string.Empty;
        public string GroupCode { get => IdentifierCode; set => IdentifierCode = value; }
        public int SortOrder { get; set; } = 1;
        public OrganizationUnitType UnitType { get; set; } = OrganizationUnitType.DonVi;
        public OrganizationUnitGroup UnitGroup { get; set; } = OrganizationUnitGroup.Default;
        public string? Description { get; set; }
        public string? TenDonViTrucThuoc { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avartar { get; set; }
        public Guid? ParentId { get; set; }
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Approved;
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? CreatedDate { get => Created; set => Created = value ?? DateTime.UtcNow; }
        public string? CreatedBy { get; set; }
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public List<GroupsDto>? ListChild { get; set; } = new();
    }

    public class GroupsForm
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GroupName { get => Name; set => Name = value; }
        public string ShortName { get; set; } = string.Empty;
        public string IdentifierCode { get; set; } = string.Empty;
        public string GroupCode { get => IdentifierCode; set => IdentifierCode = value; }
        public int SortOrder { get; set; } = 1;
        public OrganizationUnitType UnitType { get; set; } = OrganizationUnitType.DonVi;
        public OrganizationUnitGroup UnitGroup { get; set; } = OrganizationUnitGroup.Default;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avartar { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GroupsQuery : BaseQuery
    {
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDonVi { get; set; }
    }
}
