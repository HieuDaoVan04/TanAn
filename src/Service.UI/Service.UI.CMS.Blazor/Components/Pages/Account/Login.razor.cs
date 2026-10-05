using Microsoft.AspNetCore.Components;
namespace Service.UI.CMS.Blazor.Components.Pages.Account;
public partial class Login
{
    [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery] public string? Error { get; set; }
    private string ErrorMessage => Error == "unavailable"
        ? "Dịch vụ đăng nhập tạm thời không khả dụng. Vui lòng thử lại sau."
        : "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đang bị khóa.";
}
