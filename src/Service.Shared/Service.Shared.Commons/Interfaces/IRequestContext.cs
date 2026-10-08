// "Một sản phẩm của HieuDV"

using Service.Shared.Commons.Models;

namespace Service.Shared.Commons.Interfaces
{
    /// <summary>
    /// Interface quản lý ngữ cảnh yêu cầu và thông tin người dùng đăng nhập
    /// Tác giả: HieuDV Pattern
    /// </summary>
    public interface IRequestContext
    {
        CurrentUserDto CurrentUser { get; }
    }
}

