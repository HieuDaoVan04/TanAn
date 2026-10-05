// "Một sản phẩm của HieuDV"

using Service.TanAn.Domain.Interfaces.MSSQL;

namespace Service.TanAn.Domain.Interfaces
{
    public interface IUnitOfWorkQuanTriHeThong : IUnitOfWork
    {
        ILogThaoTacNguoiDungRepository LogThaoTacNguoiDungRepository { get; }
        IModuleRepository ModuleRepository { get; }
        IRoleModulesRepository RoleModulesRepository { get; }
        IPermissionRepository PermissionRepository { get; }
        IRoleRepository RoleRepository { get; }
        IRolePermissionRepository RolePermissionRepository { get; }
        IUserRoleHistoryRepository UserRoleHistoryRepository { get; }
        ISystemParameterRepository SystemParameterRepository { get; }
        IUserRepository UserRepository { get; }
        IGroupsRepository GroupsRepository { get; }
        IUserGroupsRepository UserGroupsRepository { get; }
    }
}
