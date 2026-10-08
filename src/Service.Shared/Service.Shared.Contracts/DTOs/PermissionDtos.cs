// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class PermissionDto : BaseEntiyDto
    {
        public int STT { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public EnumPermissions PermissionCode { get; set; }
        public string? Code { get => PermissionCode.ToString(); set { if (Enum.TryParse<EnumPermissions>(value, out var res)) PermissionCode = res; } }
        public string? Name { get => PermissionName; set => PermissionName = value ?? string.Empty; }
        public string? Description { get; set; } = string.Empty;
        public bool IsSync { get; set; }
        public string? PermissionParentName { get; set; }
        public bool IsSelected { get; set; }
        public Guid? ParentId { get; set; }
        public List<PermissionDto> ListChild { get; set; } = new List<PermissionDto>();
    }

    public class PermissionForm
    {
        public Guid Id { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public EnumPermissions PermissionCode { get; set; }
        public Guid? PermissionParentId { get; set; }
        public bool IsSync { get; set; }
    }

    public class PermissionQuery : BaseQuery
    {
        public string? PermissionName { get; set; }
        public EnumPermissions? PermissionCode { get; set; }
    }
}
