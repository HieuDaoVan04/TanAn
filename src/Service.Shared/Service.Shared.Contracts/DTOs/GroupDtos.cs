using System;
using System.Collections.Generic;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Querys.Grid;

namespace Service.Shared.Contracts.DTOs
{
    public class GroupDto : BaseEntiyDto
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MaGroup { get; set; } = string.Empty;
        public GroupDto? Parent { get; set; }
        public Guid? ParentId { get; set; }
        public string? Description { get; set; }
        public EnumLoaiGroup LoaiGroup { get; set; } = EnumLoaiGroup.DonVi;
        public int? Quota { get; set; }
    }

    public class GroupTreeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MaGroup { get; set; } = string.Empty;
        public EnumLoaiGroup LoaiGroup { get; set; } = EnumLoaiGroup.DonVi;
        public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.Approved;
        public List<GroupTreeDto>? Children { get; set; } = new();
    }

    public class GroupQuery : BaseQuery
    {
        public EnumLoaiGroup? LoaiGroup { get; set; }
        public List<Sort>? sort { get; set; }
    }
}
