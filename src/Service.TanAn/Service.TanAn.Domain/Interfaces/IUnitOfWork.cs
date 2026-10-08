// "Một sản phẩm của HieuDV"

using System.Threading;
using System.Threading.Tasks;
using Service.Shared.Commons.Interfaces.SQL;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Domain.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IRepository<User> UserRepository { get; }
        IRepository<AuditLog> AuditLogRepository { get; }
        IRepository<YeuCauNguoiDan> YeuCauNguoiDanRepository { get; }
        IRepository<NhanKhau> NhanKhauRepository { get; }
        IRepository<HoGiaDinh> HoGiaDinhRepository { get; }
        IRepository<DoiTuongAnSinh> DoiTuongAnSinhRepository { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
