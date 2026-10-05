namespace Service.Shared.Contracts.DTOs;

public enum AdminCatalog { Modules, Menus, Users, Roles, Groups, Parameters }

public class AdminRecord
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; } = true;
    public Service.Shared.Commons.Model.SQL.ModerationStatus? ModerationStatus { get; set; }
    public Guid? ParentId { get; set; }
    public int UnitType { get; set; }
    public Guid? ModuleId { get; set; }
    public string? Path { get; set; }
    public string? Icon { get; set; }
    public int Order { get; set; }
    public bool Expanded { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    // Chỉ dùng cho form gửi lên, không trả mật khẩu từ truy vấn danh sách.
    public string Password { get; set; } = string.Empty;
    public Service.TanAn.Domain.Enums.RoleEnum AccountRole { get; set; } = Service.TanAn.Domain.Enums.RoleEnum.CanBoXa;
    public List<Guid> VillageIds { get; set; } = new();
    public List<Guid> AssignedIds { get; set; } = new();
}
