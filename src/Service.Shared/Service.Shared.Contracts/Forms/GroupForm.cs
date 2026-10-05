using System;
using System.ComponentModel.DataAnnotations;

namespace Service.Shared.Contracts.Forms
{
    public class GroupForm
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên đơn vị/hệ thống không được để trống")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã đơn vị/hệ thống không được để trống")]
        public string MaGroup { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }
        public int? Quota { get; set; }
        public string? Description { get; set; }
    }
}
