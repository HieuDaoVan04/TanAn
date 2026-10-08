// "Một sản phẩm của HieuDV"

using System.Threading;
using System.Threading.Tasks;
using Service.Shared.Commons.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces;
using Service.TanAn.Domain.Interfaces.MSSQL;
using Service.TanAn.Infrastructure.Persistence;
using Service.TanAn.Infrastructure.Repositories.MSSQL;

namespace Service.TanAn.Infrastructure.Repositories.Bases
{
    /// <summary>
    /// Implements the Unit of Work pattern to manage data persistence and transactions
    /// Author: HieuDV Pattern
    /// </summary>
    public sealed class UnitOfWork : BaseUnitOfWork, 
        IUnitOfWork, 
        IUnitOfWorkQuanTriHeThong,
        IUnitOfWorkDanCu,
        IUnitOfWorkTTHC,
        IUnitOfWorkAnSinh,
        IUnitOfWorkChatBot
    {
        private readonly TanAnDbContext _context;

        private IRepository<User>? _userRepository;
        private IRepository<AuditLog>? _auditLogRepository;
        private IRepository<YeuCauNguoiDan>? _yeuCauNguoiDanRepository;
        private IRepository<NhanKhau>? _nhanKhauRepository;
        private IRepository<HoGiaDinh>? _hoGiaDinhRepository;
        private IRepository<DoiTuongAnSinh>? _doiTuongAnSinhRepository;
        private ILogThaoTacNguoiDungRepository? _logThaoTacNguoiDungRepository;
        private IModuleRepository? _moduleRepository;
        private IRoleModulesRepository? _roleModulesRepository;
        private IPermissionRepository? _permissionRepository;
        private IRoleRepository? _roleRepository;
        private IRolePermissionRepository? _rolePermissionRepository;
        private IUserRoleHistoryRepository? _userRoleHistoryRepository;
        private ISystemParameterRepository? _systemParameterRepository;
        private IUserRepository? _userRepositoryCustom;
        private IGroupsRepository? _groupsRepository;
        private IUserGroupsRepository? _userGroupsRepository;

        public UnitOfWork(TanAnDbContext context, IRequestContext requestContext)
            : base(context, requestContext)
        {
            _context = context;
        }

        IRepository<User> IUnitOfWork.UserRepository => UserRepository;

        public IUserRepository UserRepository =>
            _userRepositoryCustom ??= new UserRepository(_context);

        public IGroupsRepository GroupsRepository =>
            _groupsRepository ??= new GroupsRepository(_context);

        public IUserGroupsRepository UserGroupsRepository =>
            _userGroupsRepository ??= new UserGroupsRepository(_context);

        public IRepository<AuditLog> AuditLogRepository =>
            _auditLogRepository ??= new EfRepository<AuditLog>(_context);

        public IRepository<YeuCauNguoiDan> YeuCauNguoiDanRepository =>
            _yeuCauNguoiDanRepository ??= new EfRepository<YeuCauNguoiDan>(_context);

        public IRepository<NhanKhau> NhanKhauRepository =>
            _nhanKhauRepository ??= new EfRepository<NhanKhau>(_context);

        public IRepository<HoGiaDinh> HoGiaDinhRepository =>
            _hoGiaDinhRepository ??= new EfRepository<HoGiaDinh>(_context);

        public IRepository<DoiTuongAnSinh> DoiTuongAnSinhRepository =>
            _doiTuongAnSinhRepository ??= new EfRepository<DoiTuongAnSinh>(_context);

        public ILogThaoTacNguoiDungRepository LogThaoTacNguoiDungRepository =>
            _logThaoTacNguoiDungRepository ??= new LogThaoTacNguoiDungRepository(_context);

        public IModuleRepository ModuleRepository =>
            _moduleRepository ??= new ModuleRepository(_context);

        public IRoleModulesRepository RoleModulesRepository =>
            _roleModulesRepository ??= new RoleModulesRepository(_context);

        public IPermissionRepository PermissionRepository =>
            _permissionRepository ??= new PermissionRepository(_context);

        public IRoleRepository RoleRepository =>
            _roleRepository ??= new RoleRepository(_context);

        public IRolePermissionRepository RolePermissionRepository =>
            _rolePermissionRepository ??= new RolePermissionRepository(_context);

        public IUserRoleHistoryRepository UserRoleHistoryRepository =>
            _userRoleHistoryRepository ??= new UserRoleHistoryRepository(_context);

        public ISystemParameterRepository SystemParameterRepository =>
            _systemParameterRepository ??= new SystemParameterRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await CompleteAsync();
        }
    }
}
